using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Kafka.Client.Connection.Protocol.Metadata;

namespace Kafka.Client.Metadata.Store
{
	public class MetadataTopicStore
	{
		private readonly ConcurrentDictionary<int, int> partitionLeaders = new ConcurrentDictionary<int, int>();

		public void UpdateMetadata(IEnumerable<PartitionMetadata> partitions)
		{
			foreach (var partition in partitions)
			{
				if (partition.PartitionErrorCode != 0)
					throw new InvalidOperationException(string.Format("Invalid partition metadata item: {0}", partition));
				partitionLeaders.AddOrUpdate(partition.PartitionId, partition.Leader, (id, oldLeader) => partition.Leader);
			}
		}

		public int GetPartitionLeaderNodeId(int partitionId)
		{
			int leaderNodeId;
			if (!partitionLeaders.TryGetValue(partitionId, out leaderNodeId))
				throw new InvalidOperationException(string.Format("Partition with id {0} was not found", partitionId));
			return leaderNodeId;
		}
	}
}