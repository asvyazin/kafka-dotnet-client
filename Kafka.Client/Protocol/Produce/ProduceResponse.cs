using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.Produce
{
	public class ProduceResponse: ResponseMessage
	{
		private ProduceResponse(ProduceResponseTopicItem[] topicItems)
		{
			TopicItems = topicItems;
		}

		public ProduceResponseTopicItem[] TopicItems { get; private set; }

		public static ProduceResponse FromStream(Stream stream)
		{
			var topicItems = stream.ReadArray(ProduceResponseTopicItem.FromStream);
			return new ProduceResponse(topicItems);
		}
	}
}