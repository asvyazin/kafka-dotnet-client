using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.OffsetCommit
{
	public class OffsetCommitResponse: ResponseMessage
	{
		public string ClientId { get; private set; }
		public OffsetCommitResponseTopicItem[] TopicItems { get; private set; }

		private OffsetCommitResponse(string clientId, OffsetCommitResponseTopicItem[] topicItems)
		{
			ClientId = clientId;
			TopicItems = topicItems;
		}

		public static OffsetCommitResponse FromStream(Stream stream)
		{
			var clientId = stream.ReadString();
			var topicItems = stream.ReadArray(OffsetCommitResponseTopicItem.FromStream);
			return new OffsetCommitResponse(clientId, topicItems);
		}
	}
}