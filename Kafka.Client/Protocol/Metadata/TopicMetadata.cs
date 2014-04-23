using System;
using System.IO;
using System.Linq;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.Metadata
{
	public class TopicMetadata
	{
		private TopicMetadata(Int16 topicErrorCode, string topicName, PartitionMetadata[] partitionsMetadata)
		{
			TopicErrorCode = topicErrorCode;
			TopicName = topicName;
			PartitionsMetadata = partitionsMetadata;
		}

		public PartitionMetadata[] PartitionsMetadata { get; set; }

		public string TopicName { get; set; }

		public Int16 TopicErrorCode { get; set; }

		public static TopicMetadata FromStream(Stream stream)
		{
			var topicErrorCode = stream.ReadInt16();
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