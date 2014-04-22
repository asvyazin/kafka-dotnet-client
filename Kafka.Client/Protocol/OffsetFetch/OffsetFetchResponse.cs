using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.OffsetFetch
{
	public class OffsetFetchResponse: ResponseMessage
	{
		public string ClientId { get; private set; }
		public OffsetFetchResponseTopicItem[] TopicItems { get; private set; }

		private OffsetFetchResponse(string clientId, OffsetFetchResponseTopicItem[] topicItems)
		{
			ClientId = clientId;
			TopicItems = topicItems;
		}

		public static OffsetFetchResponse FromStream(Stream stream)
		{
			var clientId = stream.ReadString();
			var topicItems = stream.ReadArray(OffsetFetchResponseTopicItem.FromStream);
			return new OffsetFetchResponse(clientId, topicItems);
		}
	}
}