using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Metadata
{
	public class BrokerMetadata
	{
		private BrokerMetadata(int nodeId, string host, int port)
		{
			NodeId = nodeId;
			Host = host;
			Port = port;
		}

		public int Port { get; set; }

		public string Host { get; set; }

		public int NodeId { get; set; }

		public static BrokerMetadata FromStream(Stream stream)
		{
			var nodeId = stream.ReadInt32();
			var host = stream.ReadString();
			var port = stream.ReadInt32();
			return new BrokerMetadata(nodeId, host, port);
		}
	}
}