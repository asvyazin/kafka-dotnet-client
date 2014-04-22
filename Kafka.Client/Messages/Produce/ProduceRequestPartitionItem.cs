using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Produce
{
	public class ProduceRequestPartitionItem : IWriteable
	{
		public ProduceRequestPartitionItem(int partition, MessageSetItem[] messageSet)
		{
			this.messageSet = messageSet;
			this.partition = partition;
		}

		public void Write(Stream stream)
		{
			stream.WriteInt32(partition);
			MessageSetItem.WriteMessageSet(stream, messageSet);
		}

		private readonly MessageSetItem[] messageSet;

		private readonly int partition;
	}
}