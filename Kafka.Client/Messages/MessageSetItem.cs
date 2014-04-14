using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages
{
	public class MessageSetItem
	{
		public MessageSetItem(Int64 offset, Message message)
		{
			Message = message;
			Offset = offset;
		}

		public void Write(Stream stream)
		{
			stream.WriteInt64(Offset);
			stream.WriteInt32(Message.Size);
			Message.Write(stream);
		}

		public Message Message { get; private set; }

		public Int64 Offset { get; private set; }
	}
}