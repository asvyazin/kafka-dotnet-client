using System.IO;

namespace Kafka.Client.Messages
{
	public abstract class RequestMessage: IWriteable
	{
		protected RequestMessage(ApiKey apiKey)
		{
			ApiKey = apiKey;
		}

		public ApiKey ApiKey { get; private set; }

		public abstract void Write(Stream stream);
	}
}