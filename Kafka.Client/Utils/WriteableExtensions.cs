using System.IO;

namespace Kafka.Client.Utils
{
	public static class WriteableExtensions
	{
		public static byte[] ToBytes(this IWriteable writeable)
		{
			using (var ms = new MemoryStream())
			{
				writeable.Write(ms);
				return ms.ToArray();
			}
		}
	}
}