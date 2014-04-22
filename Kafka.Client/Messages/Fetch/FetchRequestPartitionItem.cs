using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Fetch
{
	public class FetchRequestPartitionItem: IWriteable
	{
		private readonly int partitionId;
		private readonly long offset;
		private readonly int maxBytes;

		public FetchRequestPartitionItem(int partitionId, long offset, int maxBytes)
		{
			this.partitionId = partitionId;
			this.offset = offset;
			this.maxBytes = maxBytes;
		}

		public void Write(Stream stream)
		{
			stream.WriteInt32(partitionId);
			stream.WriteInt64(offset);
			stream.WriteInt32(maxBytes);
		}
	}
}