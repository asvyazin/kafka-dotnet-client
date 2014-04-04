using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Client.Utils
{
	public static class StreamExtensions
	{
		public static async Task ReadExactlyAsync(this Stream stream, byte[] buffer, int offset, int count)
		{
			var bytesRead = await stream.ReadAsync(buffer, offset, count);
			if (bytesRead != count)
				throw new InvalidOperationException(string.Format("Stream read error: expected {0} bytes but has been read {1}", count, bytesRead));
		}

		public static void ReadExactly(this Stream stream, byte[] buffer, int offset, int count)
		{
			var bytesRead = stream.Read(buffer, offset, count);
			if (bytesRead != count)
				throw new InvalidOperationException(string.Format("Stream read error: expected {0} bytes but has been read {1}", count, bytesRead));
		}

		public static async Task<Int32> ReadInt32Async(this Stream stream)
		{
			const int length = sizeof (Int32);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length);
			return BitConverter.ToInt32(buffer.Reverse().ToArray(), 0);
		}

		public static Int32 ReadInt32(this Stream stream)
		{
			const int length = sizeof (Int32);
			var buffer = new byte[length];
			stream.ReadExactly(buffer, 0, length);
			return BitConverter.ToInt32(buffer.Reverse().ToArray(), 0);
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

		public static async Task<byte> ReadByteAsync(this Stream stream)
		{
			const int length = sizeof(byte);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length);
			return buffer[0];
		}

		public static async Task<byte[]> ReadBytesAsync(this Stream stream)
		{
			var length = await stream.ReadInt32Async();
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length);
			return buffer;
		}

		public static async Task WriteBytesAsync(this Stream stream, byte[] value)
		{
			await stream.WriteInt32Async(value.Length);
			await stream.WriteAsync(value, 0, value.Length);
		}

		public static void WriteBytes(this Stream stream, byte[] value)
		{
			stream.WriteInt32(value.Length);
			stream.Write(value, 0, value.Length);
		}

		public static void WriteString(this Stream stream, string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			stream.WriteInt16((Int16)bytes.Length);
			stream.Write(bytes, 0, bytes.Length);
		}

		public static string ReadString(this Stream stream)
		{
			var length = stream.ReadInt16();
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

		public static void WriteArray<T>(this Stream stream, T[] values, Action<Stream, T> writeValue)
		{
			stream.WriteInt32(values.Length);
			foreach (var t in values)
				writeValue(stream, t);
		}
	}
}