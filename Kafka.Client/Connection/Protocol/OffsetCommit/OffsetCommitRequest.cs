using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.OffsetCommit
{
	public class OffsetCommitRequest: RequestMessage
	{
		private const Int16 Version = 0;
		public OffsetCommitRequest(string consumerGroup, OffsetCommitRequestTopicItem[] topicItems)
			: base(ApiKey.OffsetCommitRequest, Version)
		{
			this.topicItems = topicItems;
			this.consumerGroup = consumerGroup;
		}

		public override void Write(Stream stream)
		{
			stream.WriteString(consumerGroup);
			stream.WriteArray(topicItems);
		}

		private readonly OffsetCommitRequestTopicItem[] topicItems;

		private readonly string consumerGroup;
	}
}