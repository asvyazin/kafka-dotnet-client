using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.OffsetCommit
{
	public class OffsetCommitRequest: RequestMessage
	{
		public OffsetCommitRequest(string consumerGroup, OffsetCommitRequestTopicItem[] topicItems) : base(ApiKey.OffsetCommitRequest)
		{
			this.topicItems = topicItems;
			this.consumerGroup = consumerGroup;
		}

		public override void WriteMessage(Stream stream)
		{
			stream.WriteString(consumerGroup);
			stream.WriteArray(topicItems, (s, i) => i.Write(s));
		}

		private readonly OffsetCommitRequestTopicItem[] topicItems;

		private readonly string consumerGroup;
	}
}