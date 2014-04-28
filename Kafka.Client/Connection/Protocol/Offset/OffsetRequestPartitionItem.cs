using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Offset
{
	public class OffsetRequestPartitionItem: IWriteable
	{
		public OffsetRequestPartitionItem(Int32 partitionId, Int64 time, Int32 maxNumberOfOffsets)
		{
			this.maxNumberOfOffsets = maxNumberOfOffsets;
			this.time = time;
			this.partitionId = partitionId;
		}

		public void Write(Stream stream)
		{
			stream.WriteInt32(partitionId);
			stream.WriteInt64(time);
			stream.WriteInt32(maxNumberOfOffsets);
		}

		private readonly Int32 maxNumberOfOffsets;

		private readonly Int64 time;

		private readonly Int32 partitionId;
	}
}