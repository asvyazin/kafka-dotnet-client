using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Kafka.Client.Connection;
using Kafka.Client.Connection.Protocol.Fetch;
using Kafka.Client.Metadata;
using Kafka.Client.Producer;

namespace Kafka.Client.Consumer
{
	public class KafkaConsumer<TKey, TValue>: IDisposable
	{
		private readonly IDecoder<TKey> keyDecoder;
		private readonly IDecoder<TValue> valueDecoder;
		private readonly BrokerConnectionManager brokerConnectionManager;
		private readonly MetadataManager metadataManager;

		private bool disposed;
		private readonly object locker = new object();
		private readonly KafkaConsumerSettings settings;

		public KafkaConsumer(IDecoder<TKey> keyDecoder, IDecoder<TValue> valueDecoder, BrokerConnectionManager brokerConnectionManager, MetadataManager metadataManager, KafkaConsumerSettings settings)
		{
			this.keyDecoder = keyDecoder;
			this.valueDecoder = valueDecoder;
			this.brokerConnectionManager = brokerConnectionManager;
			this.metadataManager = metadataManager;
			this.settings = settings;
		}

		public IObservable<KeyedMessageAndOffset<TKey, TValue>> ConsumeMessages(string topic, Int32 partitionId, Int64 startOffset)
		{
			return Observable.Create<KeyedMessageAndOffset<TKey, TValue>>(
				async observer => await SubscribeToMessagesAsync(observer, topic, partitionId, startOffset));
		}

		private async Task SubscribeToMessagesAsync(IObserver<KeyedMessageAndOffset<TKey, TValue>> observer, string topic, Int32 partitionId, Int64 startOffset)
		{
			if (!metadataManager.IsKnownTopic(topic))
				await metadataManager.UpdateMetadata(new[] {topic});

			var currentOffset = startOffset;
			while (!disposed)
			{
				var fetchRequest = new FetchRequest(-1, settings.MaxWaitTime, settings.MinBytes, new[]
				{
					new FetchRequestTopicItem(topic, new []
					{
						new FetchRequestPartitionItem(partitionId, currentOffset, settings.MaxBytes)
					})
				});

				for (var i = 0; i < settings.SendRetryCount; ++i)
				{
					var brokerId = metadataManager.GetLeaderNodeId(topic, partitionId);
					var brokerAddress = metadataManager.GetBroker(brokerId);
					var brokerConnection = brokerConnectionManager.GetBrokerConnection(brokerAddress);

					var fetchResponse = (FetchResponse)await brokerConnection.SendRequestAsync(fetchRequest);
					if (fetchResponse.TopicItems.Length != 1)
						throw new InvalidOperationException(string.Format("Invalid fetch response, unexpected topic items count: {0}", fetchResponse.TopicItems.Length));
					var topicItem = fetchResponse.TopicItems.First();
					if (topicItem.TopicName != topic)
						throw new InvalidOperationException(string.Format("Invalid fetch response, unexpected topic name: {0}", topicItem.TopicName));
					if (topicItem.PartitionItems.Length != 1)
						throw new InvalidOperationException(string.Format("Invalid fetch response, unexpected partitions item count: {0}", topicItem.PartitionItems.Length));

					var partitionItem = topicItem.PartitionItems.First();
					if (partitionItem.ErrorCode != 0)
					{
						await metadataManager.UpdateMetadata(new[] { topic });
						continue;
					}

					foreach (var messageSetItem in partitionItem.Messages)
					{
						currentOffset = messageSetItem.Offset + messageSetItem.Message.Size;
						var key = keyDecoder.Decode(messageSetItem.Message.Key);
						var value = valueDecoder.Decode(messageSetItem.Message.Value);
						var keyedMessage = new KeyedMessage<TKey, TValue>(topic, key, value);
						observer.OnNext(new KeyedMessageAndOffset<TKey, TValue>(messageSetItem.Offset, keyedMessage));
					}
					break;
				}
			}
		}

		public void Dispose()
		{
			lock (locker)
			{
				if (disposed)
					return;

				disposed = true;
			}
		}
	}
}