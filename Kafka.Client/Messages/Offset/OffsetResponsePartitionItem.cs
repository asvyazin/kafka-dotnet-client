using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Offset
{
	public class OffsetResponsePartitionItem
	{
		public int PartitionId { get; private set; }
		public short ErrorCode { get; private set; }
		public long[] Offsets { get; private set; }

		private OffsetResponsePartitionItem(Int32 partitionId, Int16 errorCode, Int64[] offsets)
		{
			PartitionId = partitionId;
			ErrorCode = errorCode;
			Offsets = offsets;
		}

		public static OffsetResponsePartitionItem FromStream(Stream stream)
		{
			var partitionId = stream.ReadInt32();
			var errorCode = stream.ReadInt16();
			var offsets = stream.ReadArray(s => s.ReadInt64());
			return new OffsetResponsePartitionItem(partitionId, errorCode, offsets);
		}
	}
}