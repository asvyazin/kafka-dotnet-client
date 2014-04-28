using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Offset
{
	public class OffsetResponseTopicItem
	{
		public string TopicName { get; private set; }
		public OffsetResponsePartitionItem[] PartitionItems { get; private set; }

		private OffsetResponseTopicItem(string topicName, OffsetResponsePartitionItem[] partitionItems)
		{
			TopicName = topicName;
			PartitionItems = partitionItems;
		}

		public static OffsetResponseTopicItem FromStream(Stream stream)
		{
			var topicName = stream.ReadString();
			var partitionItems = stream.ReadArray(OffsetResponsePartitionItem.FromStream);
			return new OffsetResponseTopicItem(topicName, partitionItems);
		}
	}
}