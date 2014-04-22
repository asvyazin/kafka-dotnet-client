using System.IO;
using Kafka.Client.Messages.Fetch;
using Kafka.Client.Messages.Metadata;
using Kafka.Client.Messages.Offset;
using Kafka.Client.Messages.OffsetCommit;
using Kafka.Client.Messages.OffsetFetch;
using Kafka.Client.Messages.Produce;

namespace Kafka.Client.Messages
{
	public class ResponseMessage
	{
		public static ResponseMessage FromStream(ApiKey apiKey, Stream stream)
		{
			switch (apiKey)
			{
				case ApiKey.ProduceRequest:
					return ProduceResponse.FromStream(stream);
				case ApiKey.FetchRequest:
					return FetchResponse.FromStream(stream);
				case ApiKey.OffsetRequest:
					return OffsetResponse.FromStream(stream);
				case ApiKey.MetadataRequest:
					return MetadataResponse.FromStream(stream);
				case ApiKey.OffsetCommitRequest:
					return OffsetCommitResponse.FromStream(stream);
				case ApiKey.OffsetFetchRequest:
					return OffsetFetchResponse.FromStream(stream);
				default:
					throw new UnknownApiKeyException(apiKey);
			}
		}

		public static ResponseMessage FromBytes(ApiKey apiKey, byte[] bytes)
		{
			using (var ms = new MemoryStream(bytes))
			{
				return FromStream(apiKey, ms);
			}
		}
	}
}