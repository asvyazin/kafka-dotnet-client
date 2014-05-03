using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Client.Utils
{
	public static class StreamExtensions
	{
		public static async Task ReadExactlyAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var currentCount = count;
			var currentOffset = offset;

			while (currentCount > 0)
			{
				var bytesRead = await stream.ReadAsync(buffer, currentOffset, currentCount, cancellationToken);
				if (bytesRead == 0)
					throw new InvalidOperationException("Stream read error");

				currentOffset += bytesRead;
				currentCount -= bytesRead;
			}
		}

		public static void ReadExactly(this Stream stream, byte[] buffer, int offset, int count)
		{
			var currentCount = count;
			var currentOffset = offset;

			while (currentCount > 0)
			{
				var bytesRead = stream.Read(buffer, currentOffset, currentCount);
				if (bytesRead == 0)
					throw new InvalidOperationException("Stream read error");

				currentOffset += bytesRead;
				currentCount -= bytesRead;
			}
		}

		public static async Task<int> ReadInt32Async(this Stream stream, CancellationToken cancellationToken)
		{
			const int length = sizeof (Int32);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length, cancellationToken);
			return BitConverter.ToInt32(buffer.Reverse().ToArray(), 0);
		}

		public static Int32 ReadInt32(this Stream stream)
		{
			const int length = sizeof (Int32);
			var buffer = new byte[length];
			stream.ReadExactly(buffer, 0, length);
			return BitConverter.ToInt32(buffer.Reverse().ToArray(), 0);
		}

		public static UInt32 ReadUInt32(this Stream stream)
		{
			const int length = sizeof (UInt32);
			var buffer = new byte[length];
			stream.ReadExactly(buffer, 0, length);
			return BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);
		}

		public static Int64 ReadInt64(this Stream stream)
		{
			const int length = sizeof (Int64);
			var buffer = new byte[length];
			stream.ReadExactly(buffer, 0, length);
			return BitConverter.ToInt64(buffer.Reverse().ToArray(), 0);
		}

		public static async Task WriteInt32Async(this Stream stream, Int32 value)
		{
			var buffer = BitConverter.GetBytes(value).Reverse().ToArray();
			await stream.WriteAsync(buffer, 0, buffer.Length);
		}

		public static void WriteInt32(this Stream stream, Int32 value)
		{
			var buffer = BitConverter.GetBytes(value).Reverse().ToArray();
			stream.Write(buffer, 0, buffer.Length);
		}

		public static void WriteUInt32(this Stream stream, UInt32 value)
		{
			var buffer = BitConverter.GetBytes(value).Reverse().ToArray();
			stream.Write(buffer, 0, buffer.Length);
		}

		public static void WriteInt64(this Stream stream, Int64 value)
		{
			var buffer = BitConverter.GetBytes(value).Reverse().ToArray();
			stream.Write(buffer, 0, buffer.Length);
		}

		public static void WriteInt16(this Stream stream, Int16 value)
		{
			var buffer = BitConverter.GetBytes(value).Reverse().ToArray();
			stream.Write(buffer, 0, buffer.Length);
		}

		public static Int16 ReadInt16(this Stream stream)
		{
			const int length = sizeof (Int16);
			var buffer = new byte[length];
			stream.ReadExactly(buffer, 0, length);
			return BitConverter.ToInt16(buffer.Reverse().ToArray(), 0);
		}

		public static async Task<byte> ReadByteAsync(this Stream stream, CancellationToken cancellationToken)
		{
			const int length = sizeof(byte);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length, cancellationToken);
			return buffer[0];
		}

		public static async Task<byte[]> ReadBytesAsync(this Stream stream, CancellationToken cancellationToken)
		{
			var length = await stream.ReadInt32Async(cancellationToken);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length, cancellationToken);
			return buffer;
		}

		public static byte[] ReadBytes(this Stream stream)
		{
			var length = stream.ReadInt32();
			if (length == -1)
				return null;

			var buffer = new byte[length];
			stream.ReadExactly(buffer, 0, length);
			return buffer;
		}

		public static void WriteBytes(this Stream stream, byte[] value)
		{
			if (value == null)
				stream.WriteInt32(-1);
			else
			{
				stream.WriteInt32(value.Length);
				stream.Write(value, 0, value.Length);
			}
		}

		public static void WriteString(this Stream stream, string value)
		{
			if (value == null)
				stream.WriteInt16(-1);
			else
			{
				var bytes = Encoding.UTF8.GetBytes(value);
				stream.WriteInt16((Int16)bytes.Length);
				stream.Write(bytes, 0, bytes.Length);
			}
		}

		public static string ReadString(this Stream stream)
		{
			var length = stream.ReadInt16();
			if (length == -1)
				return null;

			var bytes = new byte[length];
			stream.Read(bytes, 0, length);
			return Encoding.UTF8.GetString(bytes);
		}

		public static T[] ReadArray<T>(this Stream stream, Func<Stream, T> readValue)
		{
			var length = stream.ReadInt32();
			var result = new T[length];
			for (var i = 0; i < length; ++i)
				result[i] = readValue(stream);
			return result;
		}

		public static void WriteArray<T>(this Stream stream, T[] values) where T: IWriteable
		{
			stream.WriteArray(values, (s, i) => i.Write(s));
		}

		public static void WriteArray<T>(this Stream stream, T[] values, Action<Stream, T> writeItem)
		{
			stream.WriteInt32(values.Length);
			foreach (var t in values)
				writeItem(stream, t);
		}
	}
}