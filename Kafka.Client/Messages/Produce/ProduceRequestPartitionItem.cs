using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Produce
{
	public class ProduceRequestPartitionItem
	{
		public ProduceRequestPartitionItem(int partition, MessageSetItem[] messageSet)
		{
			MessageSet = messageSet;
			Partition = partition;
		}

		public void Write(Stream stream)
		{
			stream.WriteInt32(Partition);
			stream.WriteArray(MessageSet, (s, i) => i.Write(s));
		}

		public MessageSetItem[] MessageSet { get; private set; }

		public int Partition { get; private set; }
	}
}