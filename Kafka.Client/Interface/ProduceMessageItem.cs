using System;
using Kafka.Client.Protocol;

namespace Kafka.Client.Interface
{
	public class ProduceMessageItem
	{
		public ProduceMessageItem(string topic, int partitionId, Message data)
		{
			Topic = topic;
			PartitionId = partitionId;
			Data = data;
		}

		public string Topic { get; private set; }
		public Int32 PartitionId { get; private set; }
		public Message Data { get; private set; }
	}
}