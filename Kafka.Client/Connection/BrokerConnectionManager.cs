using System.Collections.Concurrent;
using Kafka.Client.Metadata;
using Kafka.Client.Metadata.Store;

namespace Kafka.Client.Connection
{
	public class BrokerConnectionManager
	{
		private readonly MetadataStore metadataStore;
		private readonly ConcurrentDictionary<int, BrokerConnection> connections =
			new ConcurrentDictionary<int, BrokerConnection>();

		private readonly string clientId;

		public BrokerConnectionManager(string clientId, MetadataStore metadataStore)
		{
			this.metadataStore = metadataStore;
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

			var brokerNodeAddress = metadataStore.GetNodeAddress(brokerId);
			brokerConnection = new BrokerConnection(clientId, brokerNodeAddress);
			connections.AddOrUpdate(brokerId, id => brokerConnection,
				(id, existingConnection) => brokerConnection);
			return brokerConnection;
		}
	}
}