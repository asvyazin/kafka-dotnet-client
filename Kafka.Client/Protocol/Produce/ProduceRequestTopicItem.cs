using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.Produce
{
	public class ProduceRequestTopicItem : IWriteable
	{
		public ProduceRequestTopicItem(string topicName, ProduceRequestPartitionItem[] partitionItems)
		{
			this.partitionItems = partitionItems;
			this.topicName = topicName;
		}

		public void Write(Stream stream)
		{
			stream.WriteString(topicName);
			stream.WriteArray(partitionItems);
		}

		private readonly ProduceRequestPartitionItem[] partitionItems;

		private readonly string topicName;
	}
}