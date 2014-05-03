using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.OffsetCommit
{
	public class OffsetCommitResponsePartitionItem
	{
		public int PartitionId { get; private set; }
		public ErrorCode ErrorCode { get; private set; }

		private OffsetCommitResponsePartitionItem(int partitionId, ErrorCode errorCode)
		{
			PartitionId = partitionId;
			ErrorCode = errorCode;
		}

		public static OffsetCommitResponsePartitionItem FromStream(Stream stream)
		{
			var partitionId = stream.ReadInt32();
			var errorCode = (ErrorCode) stream.ReadInt16();
			return new OffsetCommitResponsePartitionItem(partitionId, errorCode);
		}
	}
}