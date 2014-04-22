using System;
using System.Collections.Generic;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol
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

		public static void WriteMessageSet(Stream stream, IEnumerable<MessageSetItem> messageSet)
		{
			byte[] bytes;
			using (var substream = new MemoryStream())
			{
				foreach (var item in messageSet)
					item.Write(substream);
				bytes = substream.ToArray();
			}

			stream.WriteBytes(bytes);
		}

		public static IEnumerable<MessageSetItem> ReadMessageSet(Stream stream)
		{
			var messagesBytes = stream.ReadBytes();
			using (var substream = new MemoryStream(messagesBytes))
			{
				if (substream.Length == 0)
					yield break;

				yield return Read(substream);
			}
		}

		public static MessageSetItem Read(MemoryStream stream)
		{
			var offset = stream.ReadInt64();
			var message = Message.Read(stream);
			return new MessageSetItem(offset, message);
		}

		public Message Message { get; private set; }

		public Int64 Offset { get; private set; }
	}
}