using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages
{
	public abstract class RequestMessage
	{
		protected RequestMessage(ApiKey apiKey)
		{
			ApiKey = apiKey;
		}

		public ApiKey ApiKey { get; private set; }

		public void Write(Stream stream)
		{
			var length = GetBytesCount();
			stream.WriteInt32(length);
			WriteMessage(stream);
		}

		protected abstract int GetBytesCount();
		protected abstract void WriteMessage(Stream stream);
	}
}