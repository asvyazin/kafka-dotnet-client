using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Raw.Protocol
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

		public static RawResponse Read(Stream stream)
		{
			var correlationId = stream.ReadInt32();
			var responseData = new byte[stream.Length - stream.Position];
			stream.Read(responseData, 0, (int)(stream.Length - stream.Position));
			return new RawResponse(correlationId, responseData);
		}
	}
}