using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.OffsetCommit
{
	public class OffsetCommitRequestPartitionItem: IWriteable
	{
		private readonly Int32 partitionId;
		private readonly Int64 offset;
		private readonly string metadata;

		public OffsetCommitRequestPartitionItem(int partitionId, long offset, string metadata)
		{
			this.partitionId = partitionId;
			this.offset = offset;
			this.metadata = metadata;
		}

		public void Write(Stream stream)
		{
			stream.WriteInt32(partitionId);
			stream.WriteInt64(offset);
			stream.WriteString(metadata);
		}
	}
}