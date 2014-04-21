using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.OffsetCommit
{
	public class OffsetCommitRequestTopicItem
	{
		public OffsetCommitRequestTopicItem(string topicName, OffsetCommitRequestPartitionItem[] partitionItems)
		{
			this.partitionItems = partitionItems;
			this.topicName = topicName;
		}

		public void Write(Stream stream)
		{
			stream.WriteString(topicName);
			stream.WriteArray(partitionItems, (s, i) => i.Write(s));
		}

		private readonly OffsetCommitRequestPartitionItem[] partitionItems;

		private readonly string topicName;
	}
}