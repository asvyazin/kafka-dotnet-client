using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Produce
{
	public class ProduceResponseTopicItem
	{
		public string TopicName { get; private set; }
		public ProduceResponsePartitionItem[] PartitionItems { get; private set; }

		private ProduceResponseTopicItem(string topicName, ProduceResponsePartitionItem[] partitionItems)
		{
			TopicName = topicName;
			PartitionItems = partitionItems;
		}

		public static ProduceResponseTopicItem FromStream(Stream stream)
		{
			var topicName = stream.ReadString();
			var partitionItems = stream.ReadArray(ProduceResponsePartitionItem.FromStream);
			return new ProduceResponseTopicItem(topicName, partitionItems);
		}
	}
}