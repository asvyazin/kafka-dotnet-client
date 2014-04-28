using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Kafka.Client.Connection.Protocol.Metadata;

namespace Kafka.Client.Metadata.Store
{
	public class MetadataStore
	{
		private readonly ConcurrentDictionary<int, BrokerAddress> nodes = new ConcurrentDictionary<int, BrokerAddress>();
		private readonly ConcurrentDictionary<string, MetadataTopicStore> topicManagers = new ConcurrentDictionary<string, MetadataTopicStore>();
		private readonly ConcurrentDictionary<string, int> partitionCounters = new ConcurrentDictionary<string, int>();

		public bool IsKnownTopic(string topic)
		{
			return topicManagers.ContainsKey(topic);
		}

		public IEnumerable<BrokerAddress> GetAllBrokers()
		{
			return nodes.Values;
		}

		public void UpdateMetadata(MetadataResponse metadataResponse)
		{
			foreach (var brokerMetadata in metadataResponse.Brokers)
			{
				var brokerAddress = brokerMetadata.ToBrokerAddress();
				nodes.AddOrUpdate(brokerMetadata.NodeId, brokerAddress, (id, oldBrokerAddress) => brokerAddress);
			}

			foreach (var topic in metadataResponse.Topics)
			{
				if (topic.TopicErrorCode != 0)
					throw new InvalidOperationException(string.Format("Invalid topic metadata item: {0}", topic));
				var topicManager = topicManagers.GetOrAdd(topic.TopicName, topicName => new MetadataTopicStore());
				topicManager.UpdateMetadata(topic.PartitionsMetadata);
				var partitionsCount = topic.PartitionsMetadata.Length;
				partitionCounters.AddOrUpdate(topic.TopicName, t => partitionsCount, (t, x) => partitionsCount);
			}
		}

		public int GetLeaderNodeId(string topic, int partitionId)
		{
			MetadataTopicStore topicStore;
			if (!topicManagers.TryGetValue(topic, out topicStore))
				throw new InvalidOperationException(string.Format("Topic {0} was not found", topic));
			return topicStore.GetPartitionLeaderNodeId(partitionId);
		}

		public BrokerAddress GetBroker(int nodeId)
		{
			BrokerAddress brokerAddress;
			if (!nodes.TryGetValue(nodeId, out brokerAddress))
				throw new InvalidOperationException(string.Format("Broker with nodeId {0} was not found", nodeId));
			return brokerAddress;
		}

		public int GetPartitionsCount(string topic)
		{
			int partitionsCount;
			if (!partitionCounters.TryGetValue(topic, out partitionsCount))
				throw new InvalidOperationException(string.Format("Topic {0} was not found", topic));
			return partitionsCount;
		}
	}
}