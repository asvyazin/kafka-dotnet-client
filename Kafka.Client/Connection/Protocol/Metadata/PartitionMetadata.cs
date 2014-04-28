using System.IO;
using System.Linq;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Metadata
{
	public class PartitionMetadata
	{
		private PartitionMetadata(short partitionErrorCode, int partitionId, int leader, int[] replicas, int[] isrs)
		{
			PartitionErrorCode = partitionErrorCode;
			PartitionId = partitionId;
			Leader = leader;
			Replicas = replicas;
			Isrs = isrs;
		}

		public int[] Isrs { get; set; }

		public int[] Replicas { get; set; }

		public int Leader { get; set; }

		public int PartitionId { get; set; }

		public short PartitionErrorCode { get; set; }

		public static PartitionMetadata ReadStream(Stream stream)
		{
			var partitionErrorCode = stream.ReadInt16();
			var partitionId = stream.ReadInt32();
			var leader = stream.ReadInt32();
			var replicas = stream.ReadArray(s => s.ReadInt32());
			var isrs = stream.ReadArray(s => s.ReadInt32());
			return new PartitionMetadata(partitionErrorCode, partitionId, leader, replicas, isrs);
		}

		public override string ToString()
		{
			var isrs = string.Join(", ", Isrs.Select(i => i.ToString()).ToArray());
			var replicas = string.Join(", ", Replicas.Select(r => r.ToString()).ToArray());
			return string.Format("Isrs: [{0}], Replicas: [{1}], Leader: {2}, PartitionId: {3}, PartitionErrorCode: {4}", isrs, replicas, Leader, PartitionId, PartitionErrorCode);
		}
	}
}