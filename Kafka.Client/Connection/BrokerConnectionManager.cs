using System.Collections.Concurrent;
using Kafka.Client.Metadata;
using Kafka.Client.Protocol;

namespace Kafka.Client.Connection
{
	public class BrokerConnectionManager
	{
		private readonly MetadataManager metadataManager;
		private readonly ConcurrentDictionary<int, BrokerConnection> connections =
			new ConcurrentDictionary<int, BrokerConnection>();

		private readonly string clientId;

		public BrokerConnectionManager(string clientId, MetadataManager metadataManager)
		{
			this.metadataManager = metadataManager;
			this.clientId = clientId;
		}

		public BrokerConnection GetBrokerConnection(int brokerId)
		{
			BrokerConnection brokerConnection;
			if (connections.TryGetValue(brokerId, out brokerConnection))
			{
				if (!brokerConnection.IsDisposed())
					return brokerConnection;
			}

			var brokerNodeAddress = metadataManager.GetNodeAddress(brokerId);
			brokerConnection = new BrokerConnection(clientId, brokerNodeAddress);
			connections.AddOrUpdate(brokerId, id => brokerConnection,
				(id, existingConnection) => brokerConnection);
			return brokerConnection;
		}
	}
}