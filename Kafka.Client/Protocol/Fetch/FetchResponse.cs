using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.Fetch
{
	public class FetchResponse: ResponseMessage
	{
		public FetchResponseTopicItem[] TopicItems { get; private set; }

		private FetchResponse(FetchResponseTopicItem[] topicItems)
		{
			TopicItems = topicItems;
		}

		public static FetchResponse FromStream(Stream stream)
		{
			var topicItems = stream.ReadArray(FetchResponseTopicItem.FromStream);
			return new FetchResponse(topicItems);
		}
	}
}