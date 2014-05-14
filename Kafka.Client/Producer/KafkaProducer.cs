using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kafka.Client.Connection;
using Kafka.Client.Connection.Protocol;
using Kafka.Client.Connection.Protocol.Produce;
using Kafka.Client.Metadata;

namespace Kafka.Client.Producer
{
	public class KafkaProducer<TKey, TValue>
	{
		private readonly IEncoder<TKey> keyEncoder;
		private readonly IEncoder<TValue> valueEncoder;
		private readonly IPartitioner<TKey> partitioner;
		private readonly BrokerConnectionManager brokerConnectionManager;
		private readonly KafkaProducerSettings settings;
		private readonly MetadataManager metadataManager;

		public KafkaProducer(IEncoder<TKey> keyEncoder, IEncoder<TValue> valueEncoder, IPartitioner<TKey> partitioner, BrokerConnectionManager brokerConnectionManager, MetadataManager metadataManager, KafkaProducerSettings settings)
		{
			this.keyEncoder = keyEncoder;
			this.valueEncoder = valueEncoder;
			this.partitioner = partitioner;
			this.brokerConnectionManager = brokerConnectionManager;
			this.settings = settings;
			this.metadataManager = metadataManager;
		}

		public async Task SendMessagesAsync(KeyedMessage<TKey, TValue>[] messages)
		{
			var unknownTopics = messages.Select(m => m.Topic).Where(t => !metadataManager.IsKnownTopic(t)).Distinct().ToArray();
			if (unknownTopics.Any())
				await metadataManager.UpdateMetadata(unknownTopics);

			var notSentMessages = new List<KeyedMessageAndError<TKey, TValue>>();
			IEnumerable<KeyedMessage<TKey, TValue>> messagesToSend = messages;
			for (var i = 0; i < settings.SendRetryCount + 1; i++)
			{
				var partitionedMessages = PartitionMessages(messagesToSend).ToArray();
				var messagesMap = BuildMessagesMap(partitionedMessages);
				var connectionAndRequests = GroupMessagesToConnectionAndRequests(partitionedMessages);

				if (settings.RequiredAcks == 0)
				{
					// Special case - fire and forget. No error, no success, nothing.
					await Task.WhenAll(connectionAndRequests
						.Select(x => x.Connection.SendRequestFireAndForgetAsync(x.Request)));
					return;
				}

				var tasks = connectionAndRequests
					.Select(x => x.Connection.SendRequestAsync(x.Request));

				var responses = await Task.WhenAll(tasks);
				var produceResponses = responses.Cast<ProduceResponse>();
				var messagesToReSend = new List<KeyedMessage<TKey, TValue>>();
				var topicsToUpdate = new HashSet<string>();
				foreach (var topicItem in produceResponses.SelectMany(r => r.TopicItems))
				{
					foreach (var partitionItem in topicItem.PartitionItems)
					{
						if (partitionItem.ErrorCode == ErrorCode.NoError)
							continue;

						var key = Tuple.Create(topicItem.TopicName, partitionItem.Partition);
						var messagesWithError = messagesMap[key];

						if (partitionItem.ErrorCode == ErrorCode.NotLeaderForPartition)
						{
							messagesToReSend.AddRange(messagesWithError);
							topicsToUpdate.Add(topicItem.TopicName);
						}
						else
						{
							notSentMessages.AddRange(messagesWithError.Select(m => new KeyedMessageAndError<TKey, TValue>
							{
								Message = m,
								Error = partitionItem.ErrorCode,
							}));
						}
					}
				}

				messagesToSend = messagesToReSend;
				if (!messagesToReSend.Any())
					break;

				await metadataManager.UpdateMetadata(topicsToUpdate.ToArray());
			}

			notSentMessages.AddRange(messagesToSend.Select(m => new KeyedMessageAndError<TKey, TValue>
			{
				Message = m,
				Error = ErrorCode.Unknown,
			}));
			if (notSentMessages.Any())
				throw new SendMessagesFailedException<TKey, TValue>(notSentMessages.ToArray());
		}

		private IEnumerable<PartitionedMessage> PartitionMessages(IEnumerable<KeyedMessage<TKey, TValue>> messagesToSend)
		{
			return messagesToSend
				.Select(m => new PartitionedMessage
				{
					PartitionId = partitioner.Partition(m.Key, metadataManager.GetPartitionsCount(m.Topic)),
					Message = m,
				});
		}

		private IEnumerable<ConnectionAndRequest> GroupMessagesToConnectionAndRequests(IEnumerable<PartitionedMessage> partitionedMessages)
		{
			return partitionedMessages
				.Select(x => new
				{
					x.Message,
					x.PartitionId,
					BrokerId = metadataManager.GetLeaderNodeId(x.Message.Topic, x.PartitionId),
				})
				.GroupBy(x => x.BrokerId)
				.Select(g => new
				{
					BrokerAddress = metadataManager.GetBroker(g.Key),
					TopicItems = g
						.GroupBy(x => x.Message.Topic)
						.Select(gg => new
						{
							Topic = gg.Key,
							PartitionItems = gg
								.GroupBy(xx => xx.PartitionId)
								.Select(ggg => new
								{
									PartitionId = ggg.Key,
									MessageSetItems = ggg.Select(xxx => GetMessageSetItem(xxx.Message)).ToArray(),
								})
								.Select(xx => new ProduceRequestPartitionItem(xx.PartitionId, xx.MessageSetItems))
								.ToArray(),
						})
						.Select(x => new ProduceRequestTopicItem(x.Topic, x.PartitionItems))
						.ToArray(),
				})
				.Select(x => new ConnectionAndRequest
				{
					Connection = brokerConnectionManager.GetBrokerConnection(x.BrokerAddress),
					Request = new ProduceRequest(settings.RequiredAcks, settings.Timeout, x.TopicItems),
				});
		}

		private static Dictionary<Tuple<string, int>, KeyedMessage<TKey, TValue>[]> BuildMessagesMap(IEnumerable<PartitionedMessage> partitionedMessages)
		{
			return partitionedMessages
				.GroupBy(x => Tuple.Create(x.Message.Topic, x.PartitionId))
				.ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());
		}

		private MessageSetItem GetMessageSetItem(KeyedMessage<TKey, TValue> msg)
		{
			return new MessageSetItem(-1, new Message(keyEncoder.Encode(msg.Key), valueEncoder.Encode(msg.Value)));
		}

		private class ConnectionAndRequest
		{
			public BrokerConnection Connection { get; set; }
			public ProduceRequest Request { get; set; }
		}

		private class PartitionedMessage
		{
			public Int32 PartitionId { get; set; }
			public KeyedMessage<TKey, TValue> Message { get; set; }
		}
	}
}