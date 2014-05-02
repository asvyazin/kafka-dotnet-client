using System;
using System.IO;
using Kafka.Client.Utils;
using P4N;

namespace Kafka.Client.Connection.Protocol
{
	public class Message
	{
		private const byte VersionNumber = 0;

		public Message(byte[] key, byte[] value)
			: this(VersionNumber, 0, key, value)
		{
		}

		private Message(byte magicByte, byte attributes, byte[] key, byte[] value)
		{
			Value = value;
			Key = key;
			Attributes = attributes;
			MagicByte = magicByte;
		}

		public Int32 Size
		{
			get
			{
				var keyLength = Key != null ? Key.Length : 0;
				var valueLength = Value != null ? Value.Length : 0;
				return sizeof (UInt32) + 2*sizeof (byte) + 2*sizeof(Int32) + keyLength + valueLength;
			}
		}

		public void Write(Stream stream)
		{
			var bytes = MessageBytesWithoutCrc();
			var crc32 = ComputeCrc32(bytes);
			stream.WriteUInt32(crc32);
			stream.Write(bytes, 0, bytes.Length);
		}

		private static uint ComputeCrc32(byte[] bytes)
		{
			return CRC32.CalcArray(bytes);
		}

		private byte[] MessageBytesWithoutCrc()
		{
			using (var stream = new MemoryStream())
			{
				stream.WriteByte(MagicByte);
				stream.WriteByte(Attributes);
				stream.WriteBytes(Key);
				stream.WriteBytes(Value);
				return stream.ToArray();
			}
		}

		public static Message Read(Stream stream)
		{
			var messageBytes = stream.ReadBytes();

			var bufferSize = messageBytes.Length - sizeof(UInt32);
			var buffer = new byte[bufferSize];

			using (var substream = new MemoryStream(messageBytes))
			{
				var crc32 = substream.ReadUInt32();
				substream.ReadExactly(buffer, 0, bufferSize);
				var computedCrc32 = ComputeCrc32(buffer);
				if (computedCrc32 != crc32)
					throw new InvalidOperationException("Corrupted message: CRC32 does not match");
			}

			using (var substream = new MemoryStream(buffer))
			{
				var magicByte = (byte)substream.ReadByte();
				var attributes = (byte)substream.ReadByte();
				var key = substream.ReadBytes();
				var value = substream.ReadBytes();
				return new Message(magicByte, attributes, key, value);
			}
		}

		public byte[] Value { get; private set; }

		public byte[] Key { get; private set; }

		public byte Attributes { get; private set; }

		public byte MagicByte { get; private set; }
	}
}