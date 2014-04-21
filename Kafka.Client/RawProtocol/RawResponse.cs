using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.RawProtocol
{
	public class RawResponse
	{
		public RawResponse(Int32 correlationId, byte[] responseData)
		{
			CorrelationId = correlationId;
			ResponseData = responseData;
		}

		public Int32 CorrelationId { get; private set; }
		public byte[] ResponseData { get; private set; }

		public static RawResponse Read(MemoryStream stream)
		{
			var correlationId = stream.ReadInt32();
			var responseData = stream.ToArray();
			return new RawResponse(correlationId, responseData);
		}
	}
}