namespace Kafka.Client.Metadata
{
	public class NodeAddress
	{
		public NodeAddress(string host, int port)
		{
			Host = host;
			Port = port;
		}

		public string Host { get; private set; }
		public int Port { get; private set; }
	}
}