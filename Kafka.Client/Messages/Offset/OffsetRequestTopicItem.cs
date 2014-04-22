using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Offset
{
	public class OffsetRequestTopicItem: IWriteable
	{
		public OffsetRequestTopicItem(string topicName, OffsetRequestPartitionItem[] partitionItems)
		{
			this.partitionItems = partitionItems;
			this.topicName = topicName;
		}

		public void Write(Stream stream)
		{
			stream.WriteString(topicName);
			stream.WriteArray(partitionItems);
		}

		private readonly OffsetRequestPartitionItem[] partitionItems;

		private readonly string topicName;
	}
}