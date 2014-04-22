using System;
using System.IO;

namespace Kafka.Client.Messages
{
	public abstract class RequestMessage: IWriteable
	{
		protected RequestMessage(ApiKey apiKey, Int16 apiVersion)
		{
			ApiVersion = apiVersion;
			ApiKey = apiKey;
		}

		public ApiKey ApiKey { get; private set; }
		public Int16 ApiVersion { get; private set; }

		public abstract void Write(Stream stream);
	}
}