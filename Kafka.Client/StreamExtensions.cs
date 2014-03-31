using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Client
{
	public static class StreamExtensions
	{
		public static async Task ReadExactlyAsync(this Stream stream, byte[] buffer, int offset, int count)
		{
			var bytesRead = await stream.ReadAsync(buffer, offset, count);
			if (bytesRead != count)
				throw new InvalidOperationException(string.Format("Stream read error: expected {0} bytes but has been read {1}", count, bytesRead));
		}

		public static async Task<Int64> ReadInt64Async(this Stream stream)
		{
			const int length = sizeof(Int64);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length);
			return BitConverter.ToInt64(buffer, 0);
		}

		public static async Task WriteInt64Async(this Stream stream, Int64 value)
		{
			var buffer = BitConverter.GetBytes(value);
			await stream.WriteAsync(buffer, 0, buffer.Length);
		}

		public static async Task<Int32> ReadInt32Async(this Stream stream)
		{
			const int length = sizeof (Int32);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length);
			return BitConverter.ToInt32(buffer, 0);
		}

		public static async Task WriteInt32Async(this Stream stream, Int32 value)
		{
			var buffer = BitConverter.GetBytes(value);
			await stream.WriteAsync(buffer, 0, buffer.Length);
		}

		public static async Task<Int16> ReadInt16Async(this Stream stream)
		{
			const int length = sizeof(Int16);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length);
			return BitConverter.ToInt16(buffer, 0);
		}

		public static async Task WriteInt16Async(this Stream stream, Int16 value)
		{
			var buffer = BitConverter.GetBytes(value);
			await stream.WriteAsync(buffer, 0, buffer.Length);
		}

		public static async Task<byte> ReadByteAsync(this Stream stream)
		{
			const int length = sizeof(byte);
			var buffer = new byte[length];
			await stream.ReadExactlyAsync(buffer, 0, length);
			return buffer[0];
		}

		public static async Task WriteByteAsync(this Stream stream, byte value)
		{
			var buffer = BitConverter.GetBytes(value);
			await stream.WriteAsync(buffer, 0, buffer.Length);
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

		public static async Task<string> ReadStringAsync(this Stream stream)
		{
			var bytes = await stream.ReadBytesAsync();
			return Encoding.UTF8.GetString(bytes);
		}

		public static async Task WriteStringAsync(this Stream stream, string value)
		{
			await stream.WriteBytesAsync(Encoding.UTF8.GetBytes(value));
		}

		public static async Task<T[]> ReadArrayAsync<T>(this Stream stream, Func<Stream, Task<T>> readValueAsync)
		{
			var length = await stream.ReadInt32Async();
			var result = new T[length];
			for (var i = 0; i < length; ++i)
				result[i] = await readValueAsync(stream);
			return result;
		}
	}
}