using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Produce
{
	public class ProduceResponsePartitionItem
	{
		public int Partition { get; private set; }
		public ErrorCode ErrorCode { get; private set; }
		public long Offset { get; private set; }

		private ProduceResponsePartitionItem(int partition, ErrorCode errorCode, long offset)
		{
			Partition = partition;
			ErrorCode = errorCode;
			Offset = offset;
		}

		public static ProduceResponsePartitionItem FromStream(Stream stream)
		{
			var partition = stream.ReadInt32();
			var errorCode = (ErrorCode) stream.ReadInt16();
			var offset = stream.ReadInt64();
			return new ProduceResponsePartitionItem(partition, errorCode, offset);
		}
	}
}