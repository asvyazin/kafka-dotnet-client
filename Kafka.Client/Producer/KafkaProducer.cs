﻿using System;
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

			IEnumerable<KeyedMessage<TKey, TValue>> messagesToSend = messages;
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
					.Select(x => new
					{
						BrokerConnection = brokerConnectionManager.GetBrokerConnection(x.BrokerAddress),
						Request = new ProduceRequest(settings.RequiredAcks, settings.Timeout, x.TopicItems),
					})
					.Select(x => x.BrokerConnection.SendRequestAsync(x.Request));

				var responses = await Task.WhenAll(tasks);
				var produceResponses = responses.Cast<ProduceResponse>();
				var failedMessages = new List<KeyedMessage<TKey, TValue>>();
				var topicsToUpdate = new HashSet<string>();
				foreach (var topicItem in produceResponses.SelectMany(r => r.TopicItems))
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

				await metadataManager.UpdateMetadata(topicsToUpdate.ToArray());
				messagesToSend = failedMessages;
			}

			var notSentMessages = messagesToSend.ToArray();
			if (notSentMessages.Any())
				throw new SendMessagesFailedException<TKey, TValue>(notSentMessages);
		}

		private MessageSetItem GetMessageSetItem(KeyedMessage<TKey, TValue> msg)
		{
			return new MessageSetItem(-1, new Message(keyEncoder.Encode(msg.Key), valueEncoder.Encode(msg.Value)));
		}
	}
}