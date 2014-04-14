using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages
{
	public class Message
	{
		public Message(int crc32, byte magicByte, byte attributes, byte[] key, byte[] value)
		{
			Value = value;
			Key = key;
			Attributes = attributes;
			MagicByte = magicByte;
			Crc32 = crc32;
		}

		public Int32 Size
		{
			get { return sizeof (int) + 2*sizeof (byte) + 2*sizeof(int) + Key.Length + Value.Length; }
		}

		public void Write(Stream stream)
		{
			stream.WriteInt32(Crc32);
			stream.WriteByte(MagicByte);
			stream.WriteByte(Attributes);
			stream.WriteBytes(Key);
			stream.WriteBytes(Value);
		}

		public byte[] Value { get; private set; }

		public byte[] Key { get; private set; }

		public byte Attributes { get; private set; }

		public byte MagicByte { get; private set; }

		public Int32 Crc32 { get; private set; }
	}
}