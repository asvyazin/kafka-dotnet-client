using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Offset
{
	public class OffsetResponsePartitionItem
	{
		public int PartitionId { get; private set; }
		public ErrorCode ErrorCode { get; private set; }
		public long[] Offsets { get; private set; }

		private OffsetResponsePartitionItem(Int32 partitionId, ErrorCode errorCode, Int64[] offsets)
		{
			PartitionId = partitionId;
			ErrorCode = errorCode;
			Offsets = offsets;
		}

		public static OffsetResponsePartitionItem FromStream(Stream stream)
		{
			var partitionId = stream.ReadInt32();
			var errorCode = (ErrorCode) stream.ReadInt16();
			var offsets = stream.ReadArray(s => s.ReadInt64());
			return new OffsetResponsePartitionItem(partitionId, errorCode, offsets);
		}
	}
}