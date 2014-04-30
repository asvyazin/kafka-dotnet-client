using System.Collections.Concurrent;

namespace Kafka.Client.Connection
{
	public class BrokerConnectionManager
	{
		private readonly ConcurrentDictionary<int, BrokerConnection> connections =
			new ConcurrentDictionary<int, BrokerConnection>();

		private readonly string clientId;

		public BrokerConnectionManager(string clientId)
		{
			this.clientId = clientId;
		}

		public BrokerConnection GetBrokerConnection(BrokerAddress brokerAddress)
		{
			BrokerConnection brokerConnection;
			if (connections.TryGetValue(brokerAddress.NodeId, out brokerConnection))
			{
				if (!brokerConnection.IsDisposed())
					return brokerConnection;
			}

			brokerConnection = new BrokerConnection(clientId, brokerAddress.Endpoint);
			brokerConnection.StartAsync();
			connections.AddOrUpdate(brokerAddress.NodeId, id => brokerConnection,
				(id, existingConnection) => brokerConnection);
			return brokerConnection;
		}
	}
}