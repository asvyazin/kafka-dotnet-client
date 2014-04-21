using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.OffsetCommit
{
	public class OffsetCommitResponseTopicItem
	{
		public string TopicName { get; private set; }
		public OffsetCommitResponsePartitionItem[] PartitionItems { get; private set; }

		private OffsetCommitResponseTopicItem(string topicName, OffsetCommitResponsePartitionItem[] partitionItems)
		{
			TopicName = topicName;
			PartitionItems = partitionItems;
		}

		public static OffsetCommitResponseTopicItem FromStream(Stream stream)
		{
			var topicName = stream.ReadString();
			var partitionItems = stream.ReadArray(OffsetCommitResponsePartitionItem.FromStream);
			return new OffsetCommitResponseTopicItem(topicName, partitionItems);
		}
	}
}