using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Offset
{
	public class OffsetResponse: ResponseMessage
	{
		public OffsetResponseTopicItem[] TopicItems { get; private set; }

		private OffsetResponse(OffsetResponseTopicItem[] topicItems)
		{
			TopicItems = topicItems;
		}

		public static OffsetResponse FromStream(Stream stream)
		{
			var topicItems = stream.ReadArray(OffsetResponseTopicItem.FromStream);
			return new OffsetResponse(topicItems);
		}
	}
}