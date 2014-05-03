using System.IO;
using System.Linq;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Fetch
{
	public class FetchResponsePartitionItem
	{
		public int PartitionId { get; private set; }
		public ErrorCode ErrorCode { get; private set; }
		public long HighwaterMarkOffset { get; private set; }
		public MessageSetItem[] Messages { get; private set; }

		private FetchResponsePartitionItem(int partitionId, ErrorCode errorCode, long highwaterMarkOffset, MessageSetItem[] messages)
		{
			PartitionId = partitionId;
			ErrorCode = errorCode;
			HighwaterMarkOffset = highwaterMarkOffset;
			Messages = messages;
		}

		public static FetchResponsePartitionItem FromStream(Stream stream)
		{
			var partitionId = stream.ReadInt32();
			var errorCode = (ErrorCode) stream.ReadInt16();
			var highwaterMarkOffset = stream.ReadInt64();
			var messages = MessageSetItem.ReadMessageSet(stream).ToArray();
			return new FetchResponsePartitionItem(partitionId, errorCode, highwaterMarkOffset, messages);
		}
	}
}