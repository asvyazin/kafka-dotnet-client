using System.IO;
using System.Linq;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Metadata
{
	public class TopicMetadata
	{
		private TopicMetadata(ErrorCode topicErrorCode, string topicName, PartitionMetadata[] partitionsMetadata)
		{
			TopicErrorCode = topicErrorCode;
			TopicName = topicName;
			PartitionsMetadata = partitionsMetadata;
		}

		public PartitionMetadata[] PartitionsMetadata { get; private set; }

		public string TopicName { get; private set; }

		public ErrorCode TopicErrorCode { get; private set; }

		public static TopicMetadata FromStream(Stream stream)
		{
			var topicErrorCode = (ErrorCode)stream.ReadInt16();
			var topicName = stream.ReadString();
			var partitionsMetadata = stream.ReadArray(PartitionMetadata.ReadStream);
			return new TopicMetadata(topicErrorCode, topicName, partitionsMetadata);
		}

		public override string ToString()
		{
			var partitions = string.Join(", ", PartitionsMetadata.Select(p => p.ToString()).ToArray());
			return string.Format("PartitionsMetadata: [{0}], TopicName: {1}, TopicErrorCode: {2}", partitions, TopicName, TopicErrorCode);
		}
	}
}