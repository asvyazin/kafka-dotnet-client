using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Metadata
{
	public class MetadataResponse: ResponseMessage
	{
		public static MetadataResponse FromStream(Stream stream)
		{
			var brokers = stream.ReadArray(BrokerMetadata.FromStream);
			var topics = stream.ReadArray(TopicMetadata.FromStream);
			return new MetadataResponse(brokers, topics);
		}

		private MetadataResponse(BrokerMetadata[] brokers, TopicMetadata[] topics)
		{
			Brokers = brokers;
			Topics = topics;
		}

		public BrokerMetadata[] Brokers { get; private set; }
		public TopicMetadata[] Topics { get; private set; }
	}
}