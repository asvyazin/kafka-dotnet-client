using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Produce
{
	public class ProduceRequestTopicItem
	{
		public ProduceRequestTopicItem(string topicName, ProduceRequestPartitionItem[] partitionItems)
		{
			PartitionItems = partitionItems;
			TopicName = topicName;
		}

		public void Write(Stream stream)
		{
			stream.WriteString(TopicName);
			stream.WriteArray(PartitionItems, (s, i) => i.Write(stream));
		}

		public ProduceRequestPartitionItem[] PartitionItems { get; private set; }

		public string TopicName { get; private set; }
	}
}