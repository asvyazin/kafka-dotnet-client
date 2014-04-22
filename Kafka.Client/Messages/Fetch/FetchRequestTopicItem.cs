using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Fetch
{
	public class FetchRequestTopicItem: IWriteable
	{
		private readonly string topicName;
		private readonly FetchRequestPartitionItem[] partitionItems;

		public FetchRequestTopicItem(string topicName, FetchRequestPartitionItem[] partitionItems)
		{
			this.topicName = topicName;
			this.partitionItems = partitionItems;
		}

		public void Write(Stream stream)
		{
			stream.WriteString(topicName);
			stream.WriteArray(partitionItems);
		}
	}
}