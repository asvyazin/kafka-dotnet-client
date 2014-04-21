using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.OffsetFetch
{
	public class OffsetFetchRequest: RequestMessage
	{
		private readonly string consumerGroup;
		private readonly OffsetFetchRequestTopicItem[] topicItems;

		public OffsetFetchRequest(string consumerGroup, OffsetFetchRequestTopicItem[] topicItems) : base(ApiKey.OffsetFetchRequest)
		{
			this.consumerGroup = consumerGroup;
			this.topicItems = topicItems;
		}

		public override void WriteMessage(Stream stream)
		{
			stream.WriteString(consumerGroup);
			stream.WriteArray(topicItems, (s, i) => i.Write(s));
		}
	}
}