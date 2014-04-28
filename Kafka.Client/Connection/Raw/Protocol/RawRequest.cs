using System;
using System.IO;
using Kafka.Client.Connection.Protocol;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Raw.Protocol
{
	public class RawRequest: IWriteable
	{
		public ApiKey ApiKey { get; set; }
		public Int16 ApiVersion { get; set; }
		public Int32 CorrelationId { get; set; }
		public string ClientId { get; set; }
		public byte[] RequestData { get; set; }

		public void Write(Stream stream)
		{
			stream.WriteInt16((Int16)ApiKey);
			stream.WriteInt16(ApiVersion);
			stream.WriteInt32(CorrelationId);
			stream.WriteString(ClientId);
			stream.Write(RequestData, 0, RequestData.Length);
		}
	}
}