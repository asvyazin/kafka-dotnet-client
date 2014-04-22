using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.OffsetCommit
{
	public class OffsetCommitRequestTopicItem: IWriteable
	{
		public OffsetCommitRequestTopicItem(string topicName, OffsetCommitRequestPartitionItem[] partitionItems)
		{
			this.partitionItems = partitionItems;
			this.topicName = topicName;
		}

		public void Write(Stream stream)
		{
			stream.WriteString(topicName);
			stream.WriteArray(partitionItems);
		}

		private readonly OffsetCommitRequestPartitionItem[] partitionItems;

		private readonly string topicName;
	}
}