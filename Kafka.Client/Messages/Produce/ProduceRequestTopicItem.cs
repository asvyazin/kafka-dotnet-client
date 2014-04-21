﻿using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Produce
{
	public class ProduceRequestTopicItem
	{
		public ProduceRequestTopicItem(string topicName, ProduceRequestPartitionItem[] partitionItems)
		{
			this.partitionItems = partitionItems;
			this.topicName = topicName;
		}

		public void Write(Stream stream)
		{
			stream.WriteString(topicName);
			stream.WriteArray(partitionItems, (s, i) => i.Write(stream));
		}

		private readonly ProduceRequestPartitionItem[] partitionItems;

		private readonly string topicName;
	}
}