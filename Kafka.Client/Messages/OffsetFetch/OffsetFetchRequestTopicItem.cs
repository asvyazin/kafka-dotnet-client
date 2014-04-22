using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.OffsetFetch
{
	public class OffsetFetchRequestTopicItem : IWriteable
	{
		private readonly string topicName;
		private readonly Int32[] partitions;

		public OffsetFetchRequestTopicItem(string topicName, int[] partitions)
		{
			this.topicName = topicName;
			this.partitions = partitions;
		}

		public void Write(Stream stream)
		{
			stream.WriteString(topicName);
			stream.WriteArray(partitions, (s, i) => s.WriteInt32(i));
		}
	}
}