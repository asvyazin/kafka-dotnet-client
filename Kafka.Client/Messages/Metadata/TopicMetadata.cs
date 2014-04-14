using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Metadata
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
	}
}