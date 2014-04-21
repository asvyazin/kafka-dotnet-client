using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Fetch
{
	public class FetchResponseTopicItem
	{
		public string TopicName { get; private set; }
		public FetchResponsePartitionItem[] PartitionItems { get; private set; }

		private FetchResponseTopicItem(string topicName, FetchResponsePartitionItem[] partitionItems)
		{
			TopicName = topicName;
			PartitionItems = partitionItems;
		}

		public static FetchResponseTopicItem FromStream(Stream stream)
		{
			var topicName = stream.ReadString();
			var partitionItems = stream.ReadArray(FetchResponsePartitionItem.FromStream);
			return new FetchResponseTopicItem(topicName, partitionItems);
		}
	}
}