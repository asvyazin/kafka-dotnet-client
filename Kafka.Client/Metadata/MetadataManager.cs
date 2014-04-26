using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Kafka.Client.Protocol.Metadata;

namespace Kafka.Client.Metadata
{
	public class MetadataManager
	{
		private readonly ConcurrentDictionary<int, BrokerMetadata> nodes = new ConcurrentDictionary<int, BrokerMetadata>();
		private readonly ConcurrentDictionary<string, MetadataTopicManager> topicManagers = new ConcurrentDictionary<string, MetadataTopicManager>();
		private readonly ConcurrentDictionary<string, int> partitionCounters = new ConcurrentDictionary<string, int>();

		public bool IsKnownTopic(string topic)
		{
			return topicManagers.ContainsKey(topic);
		}

		public IEnumerable<int> GetAllBrokerIds()
		{
			return nodes.Values.Select(b => b.NodeId);
		}

		public void UpdateMetadata(MetadataResponse metadataResponse)
		{
			foreach (var broker in metadataResponse.Brokers)
				nodes.AddOrUpdate(broker.NodeId, broker, (id, oldBroker) => broker);
			foreach (var topic in metadataResponse.Topics)
			{
				if (topic.TopicErrorCode != 0)
					throw new InvalidOperationException(string.Format("Invalid topic metadata item: {0}", topic));
				var topicManager = topicManagers.GetOrAdd(topic.TopicName, topicName => new MetadataTopicManager());
				topicManager.UpdateMetadata(topic.PartitionsMetadata);
				var partitionsCount = topic.PartitionsMetadata.Length;
				partitionCounters.AddOrUpdate(topic.TopicName, t => partitionsCount, (t, x) => partitionsCount);
			}
		}

		public int GetLeaderNodeId(string topic, int partitionId)
		{
			MetadataTopicManager topicManager;
			if (!topicManagers.TryGetValue(topic, out topicManager))
				throw new InvalidOperationException(string.Format("Topic {0} was not found", topic));
			return topicManager.GetPartitionLeaderNodeId(partitionId);
		}

		public NodeAddress GetNodeAddress(int nodeId)
		{
			BrokerMetadata brokerMetadata;
			if (!nodes.TryGetValue(nodeId, out brokerMetadata))
				throw new InvalidOperationException(string.Format("Broker with nodeId {0} was not found", nodeId));
			return new NodeAddress(brokerMetadata.Host, brokerMetadata.Port);
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