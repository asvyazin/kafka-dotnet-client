using System;

namespace Kafka.Client.Protocol
{
	public class UnknownApiKeyException : Exception
	{
		public ApiKey ApiKey { get; private set; }

		public UnknownApiKeyException(ApiKey apiKey)
		{
			ApiKey = apiKey;
		}

		public override string Message
		{
			get { return string.Format("Unknown ApiKey: {0}", ApiKey); }
		}
	}
}