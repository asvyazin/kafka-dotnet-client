using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.OffsetCommit
{
	public class OffsetCommitResponsePartitionItem
	{
		public int PartitionId { get; private set; }
		public short ErrorCode { get; private set; }

		private OffsetCommitResponsePartitionItem(int partitionId, short errorCode)
		{
			PartitionId = partitionId;
			ErrorCode = errorCode;
		}

		public static OffsetCommitResponsePartitionItem FromStream(Stream stream)
		{
			var partitionId = stream.ReadInt32();
			var errorCode = stream.ReadInt16();
			return new OffsetCommitResponsePartitionItem(partitionId, errorCode);
		}
	}
}