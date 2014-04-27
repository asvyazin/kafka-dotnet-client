using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kafka.Client.Connection;
using Kafka.Client.Metadata;
using Kafka.Client.Protocol;
using Kafka.Client.Protocol.Metadata;
using Kafka.Client.Protocol.Produce;

namespace Kafka.Client.Producer
{
	public class KafkaProducer<TKey, TValue>
	{
		private readonly IEncoder<TKey> keyEncoder;
		private readonly IEncoder<TValue> valueEncoder;
		private readonly IPartitioner<TKey> partitioner;
		private readonly MetadataManager metadataManager;
		private readonly BrokerConnectionManager brokerConnectionManager;
		private readonly KafkaProducerSettings settings;

		public KafkaProducer(IEncoder<TKey> keyEncoder, IEncoder<TValue> valueEncoder, IPartitioner<TKey> partitioner, MetadataManager metadataManager, BrokerConnectionManager brokerConnectionManager, KafkaProducerSettings settings)
		{
			this.keyEncoder = keyEncoder;
			this.valueEncoder = valueEncoder;
			this.partitioner = partitioner;
			this.metadataManager = metadataManager;
			this.brokerConnectionManager = brokerConnectionManager;
			this.settings = settings;
		}

		public async Task SendMessagesAsync(IEnumerable<KeyedMessage<TKey, TValue>> messages)
		{
			var messagesToSend = messages;

			for (var i = 0; i < settings.SendRetryCount + 1; i++)
			{
				var preparedMessages = messagesToSend
					.Select(m => new
					{
						PartitionId = partitioner.Partition(m.Key, metadataManager.GetPartitionsCount(m.Topic)),
						Message = m,
					})
					.Select(x => new
					{
						x.Message,
						x.PartitionId,
						BrokerId = metadataManager.GetLeaderNodeId(x.Message.Topic, x.PartitionId),
					})
					.ToArray();

				var messagesMap = preparedMessages
					.GroupBy(x => Tuple.Create(x.Message.Topic, x.PartitionId))
					.ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());

				var tasks = preparedMessages
					.GroupBy(x => x.BrokerId)
					.Select(g => new
					{
						BrokerId = g.Key,
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
					.Select(x => new
					{
						BrokerConnection = brokerConnectionManager.GetBrokerConnection(x.BrokerId),
						Request = new ProduceRequest(settings.RequiredAcks, settings.Timeout, x.TopicItems),
					})
					.Select(x => x.BrokerConnection.SendRequestAsync(x.Request).ContinueWith(t => (ProduceResponse)t.Result));

				var responses = await Task.WhenAll(tasks);
				var failedMessages = new List<KeyedMessage<TKey, TValue>>();
				var topicsToUpdate = new HashSet<string>();
				foreach (var topicItem in responses.SelectMany(r => r.TopicItems))
				{
					var failedPartitionItems = topicItem.PartitionItems.Where(p => p.ErrorCode != 0);
					foreach (var partitionItem in failedPartitionItems)
					{
						var key = Tuple.Create(topicItem.TopicName, partitionItem.Partition);
						failedMessages.AddRange(messagesMap[key]);
						topicsToUpdate.Add(topicItem.TopicName);
					}
				}

				if (!failedMessages.Any())
					return;

				await UpdateMetadata(topicsToUpdate.ToArray());
				messagesToSend = failedMessages;
			}

			var notSentMessages = messagesToSend.ToArray();
			if (notSentMessages.Any())
				throw new SendMessagesFailedException<TKey, TValue>(notSentMessages);
		}

		private async Task UpdateMetadata(string[] topicsToUpdate)
		{
			var allBrokerIds = metadataManager.GetAllBrokerIds().ToArray();
			if (allBrokerIds.Any())
				await UpdateMetadataFrom(allBrokerIds, topicsToUpdate);
			else
				await UpdateMetadataFromBootstrapNodes(topicsToUpdate);
		}

		private async Task UpdateMetadataFrom(IEnumerable<int> nodeIds, string[] topicsToUpdate)
		{
			foreach (var nodeId in nodeIds)
			{
				BrokerConnection conn = null;
				try
				{
					conn = brokerConnectionManager.GetBrokerConnection(nodeId);
					await UpdateMetadataFromBrokerConnection(conn, topicsToUpdate);
					return;
				}
				catch (SocketException)
				{
					if (conn != null)
						conn.Dispose();
				}
			}
		}

		private async Task UpdateMetadataFromBrokerConnection(BrokerConnection conn, string[] topicsToUpdate)
		{
			var response = (MetadataResponse) await conn.SendRequestAsync(new MetadataRequest(topicsToUpdate));
			metadataManager.UpdateMetadata(response);
		}

		private async Task UpdateMetadataFromBootstrapNodes(string[] topicsToUpdate)
		{
			foreach (var node in settings.BootstrapNodes)
			{
				try
				{
					using (var conn = new BrokerConnection(settings.ClientId, node))
					{
						await UpdateMetadataFromBrokerConnection(conn, topicsToUpdate);
						return;
					}
				}
				catch (SocketException){}
			}
		}

		private MessageSetItem GetMessageSetItem(KeyedMessage<TKey, TValue> msg)
		{
			return new MessageSetItem(-1, new Message(keyEncoder.Encode(msg.Key), valueEncoder.Encode(msg.Value)));
		}
	}
}