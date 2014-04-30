using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kafka.Client.Connection;
using Kafka.Client.Connection.Protocol.Metadata;
using Kafka.Client.Metadata.Store;

namespace Kafka.Client.Metadata
{
	public class MetadataManager
	{
		private readonly MetadataStore metadataStore;
		private readonly BrokerConnectionManager brokerConnectionManager;
		private readonly MetadataManagerSettings settings;

		public MetadataManager(MetadataStore metadataStore, BrokerConnectionManager brokerConnectionManager, MetadataManagerSettings settings)
		{
			this.metadataStore = metadataStore;
			this.brokerConnectionManager = brokerConnectionManager;
			this.settings = settings;
		}

		public async Task UpdateMetadata(string[] topicsToUpdate)
		{
			var allBrokerIds = metadataStore.GetAllBrokers().ToArray();
			if (allBrokerIds.Any())
				await UpdateMetadataFrom(allBrokerIds, topicsToUpdate);
			else
				await UpdateMetadataFromBootstrapNodes(topicsToUpdate);
		}

		private async Task UpdateMetadataFrom(IEnumerable<BrokerAddress> brokers, string[] topicsToUpdate)
		{
			foreach (var nodeId in brokers)
			{
				BrokerConnection conn = null;
				try
				{
					conn = brokerConnectionManager.GetBrokerConnection(nodeId);
					await UpdateMetadataFromBrokerConnection(conn, topicsToUpdate);
					return;
				}
				catch (SocketException)
				{
					if (conn != null)
						conn.Dispose();
				}
			}
		}

		private async Task UpdateMetadataFromBrokerConnection(BrokerConnection conn, string[] topicsToUpdate)
		{
			var response = (MetadataResponse)await conn.SendRequestAsync(new MetadataRequest(topicsToUpdate));
			metadataStore.UpdateMetadata(response);
		}

		private async Task UpdateMetadataFromBootstrapNodes(string[] topicsToUpdate)
		{
			foreach (var node in settings.BootstrapNodes)
			{
				try
				{
					using (var conn = new BrokerConnection(settings.ClientId, node))
					{
						var connectionTask = conn.StartAsync();
						await UpdateMetadataFromBrokerConnection(conn, topicsToUpdate);
						return;
					}
				}
				catch (SocketException) { }
			}

			throw new InvalidOperationException("Error updating metadata from bootstrap nodes: no nodes answered");
		}

		public int GetPartitionsCount(string topic)
		{
			return metadataStore.GetPartitionsCount(topic);
		}

		public int GetLeaderNodeId(string topic, int partitionId)
		{
			return metadataStore.GetLeaderNodeId(topic, partitionId);
		}

		public BrokerAddress GetBroker(int nodeId)
		{
			return metadataStore.GetBroker(nodeId);
		}

		public bool IsKnownTopic(string topic)
		{
			return metadataStore.IsKnownTopic(topic);
		}
	}
}