using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.OffsetFetch
{
	public class OffsetFetchResponsePartitionItem
	{
		public int PartitionId { get; private set; }
		public long Offset { get; private set; }
		public string Metadata { get; private set; }
		public short ErrorCode { get; private set; }

		private OffsetFetchResponsePartitionItem(int partitionId, long offset, string metadata, short errorCode)
		{
			PartitionId = partitionId;
			Offset = offset;
			Metadata = metadata;
			ErrorCode = errorCode;
		}

		public static OffsetFetchResponsePartitionItem FromStream(Stream stream)
		{
			var partitionId = stream.ReadInt32();
			var offset = stream.ReadInt64();
			var metadata = stream.ReadString();
			var errorCode = stream.ReadInt16();
			return new OffsetFetchResponsePartitionItem(partitionId, offset, metadata, errorCode);
		}
	}
}