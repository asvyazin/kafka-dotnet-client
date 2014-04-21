﻿using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.OffsetFetch
{
	public class OffsetFetchResponseTopicItem
	{
		public string TopicName { get; private set; }
		public OffsetFetchResponsePartitionItem[] PartitionItems { get; private set; }

		private OffsetFetchResponseTopicItem(string topicName, OffsetFetchResponsePartitionItem[] partitionItems)
		{
			TopicName = topicName;
			PartitionItems = partitionItems;
		}

		public static OffsetFetchResponseTopicItem FromStream(Stream stream)
		{
			var topicName = stream.ReadString();
			var partitionItems = stream.ReadArray(OffsetFetchResponsePartitionItem.FromStream);
			return new OffsetFetchResponseTopicItem(topicName, partitionItems);
		}
	}
}