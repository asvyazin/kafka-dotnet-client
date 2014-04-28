using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol
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