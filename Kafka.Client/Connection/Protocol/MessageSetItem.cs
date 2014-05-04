using System;
using System.Collections.Generic;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol
{
	public class MessageSetItem: IWriteable
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
				MessageSetItem messageSetItem;
				while (TryRead(substream, out messageSetItem))
					yield return messageSetItem;
			}
		}

		public static bool TryRead(Stream stream, out MessageSetItem messageSetItem)
		{
			messageSetItem = null;

			Int64 offset;
			if (!stream.TryReadInt64(out offset))
				return false;

			Message message;
			if (!Message.TryRead(stream, out message))
				return false;

			messageSetItem = new MessageSetItem(offset, message);
			return true;
		}

		public Message Message { get; private set; }

		public Int64 Offset { get; private set; }

		public Int64 NextOffset
		{
			get { return Offset + 1; }
		}
	}
}