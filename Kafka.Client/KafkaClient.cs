using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kafka.Client.Connection;
using Kafka.Client.Interface;
using Kafka.Client.Metadata;
using Kafka.Client.Protocol;
using Kafka.Client.Protocol.Metadata;
using Kafka.Client.Protocol.Produce;

namespace Kafka.Client
{
	public class KafkaClient
	{
		private readonly MetadataManager metadataManager;
		private readonly BrokerConnectionManager brokerConnectionManager;
		private readonly NodeAddress[] bootstrapNodes;
		private readonly string clientId;

		public KafkaClient(string clientId, MetadataManager metadataManager, BrokerConnectionManager brokerConnectionManager, NodeAddress[] bootstrapNodes)
		{
			this.metadataManager = metadataManager;
			this.brokerConnectionManager = brokerConnectionManager;
			this.bootstrapNodes = bootstrapNodes;
			this.clientId = clientId;
		}

		public async Task<ProduceResponse[]> BatchProduceAsync(BatchProduceRequest request)
		{
			var unknownTopics = request.Messages
				.Select(m => m.Topic)
				.Where(t => !metadataManager.IsKnownTopic(t))
				.ToArray();
			if (unknownTopics.Any())
				await UpdateMetadata(unknownTopics);

			var groupedRequests = request.Messages.Select(m => new
			{
				LeaderBrokerId = metadataManager.GetLeaderNodeId(m.Topic, m.PartitionId),
				Message = m,
			})
			.GroupBy(x => x.LeaderBrokerId)
			.Select(g => new
			{
				LeaderBrokerId = g.Key,
				TopicItems = g.GroupBy(x => x.Message.Topic)
					.Select(gg => new
					{
						TopicName = gg.Key,
						PartitionItems = gg.GroupBy(xx => xx.Message.PartitionId)
							.Select(ggg => new
							{
								PartitionId = ggg.Key,
								Messages = ggg.Select(xxx => new MessageSetItem(-1, xxx.Message.Data)).ToArray(),
							})
							.Select(x => new ProduceRequestPartitionItem(x.PartitionId, x.Messages))
							.ToArray(),
					})
					.Select(x => new ProduceRequestTopicItem(x.TopicName, x.PartitionItems)).ToArray(),
			})
			.Select(x => new
			{
				x.LeaderBrokerId,
				Request = new ProduceRequest(request.RequiredAcks, request.Timeout, x.TopicItems),
			});

			var tasks = new List<Task<ProduceResponse>>();
			tasks.AddRange(groupedRequests.Select(r => brokerConnectionManager
				.GetBrokerConnection(r.LeaderBrokerId)
				.SendRequestAsync(r.Request)
				.ContinueWith(t => (ProduceResponse)t.Result)));
			return await Task.WhenAll(tasks.ToArray());
		}

		public async Task<ResponseMessage> SendRequestsAsync(string topic, int partitionId, RequestMessage request)
		{
			if (!metadataManager.IsKnownTopic(topic))
				await UpdateMetadata(topic);

			var leaderNodeId = metadataManager.GetLeaderNodeId(topic, partitionId);

			BrokerConnection brokerConnection = null;
			try
			{
				brokerConnection = brokerConnectionManager.GetBrokerConnection(leaderNodeId);
				return await brokerConnection.SendRequestAsync(request);
			}
			catch (SocketException)
			{
			}

			// node failed
			if (brokerConnection != null)
				brokerConnection.Dispose();
			await UpdateMetadata(topic);
			return await SendRequestsAsync(topic, partitionId, request);
		}

		private async Task UpdateMetadata(params string[] topics)
		{
			var knownBrokerIds = metadataManager.GetAllBrokerIds().ToArray();
			if (knownBrokerIds.Any())
				await UpdateMetadataFrom(knownBrokerIds, topics);
			else
				await UpdateMetadataFromBootstrap(topics);
		}

		private async Task UpdateMetadataFrom(IEnumerable<int> knownBrokerIds, string[] topics)
		{
			foreach (var brokerId in knownBrokerIds)
			{
				if (await TryUpdateMetadataFrom(brokerId, topics))
					break;
			}
		}

		private async Task<bool> TryUpdateMetadataFrom(int brokerId, string[] topics)
		{
			BrokerConnection conn = null;
			try
			{
				conn = brokerConnectionManager.GetBrokerConnection(brokerId);
				return await TryUpdateMetadataFromBrokerConnection(topics, conn);
			}
			catch (SocketException)
			{
				if (conn != null)
					conn.Dispose();
				return false;
			}
		}

		private async Task<bool> TryUpdateMetadataFromBrokerConnection(string[] topics, BrokerConnection conn)
		{
			var response = (MetadataResponse)await conn.SendRequestAsync(new MetadataRequest(topics));
			foreach (var topicMetadata in response.Topics)
			{
				if (topicMetadata.TopicErrorCode != 0)
					throw new InvalidOperationException(string.Format("Topic metadata error received: {0}", topicMetadata.TopicErrorCode));
				foreach (var partitionMetadata in topicMetadata.PartitionsMetadata)
				{
					if (partitionMetadata.PartitionErrorCode != 0)
						throw new InvalidOperationException(string.Format("Partition metadata error received: {0}", partitionMetadata.PartitionErrorCode));
				}
			}
			metadataManager.UpdateMetadata(response);
			return true;
		}

		private async Task UpdateMetadataFromBootstrap(string[] topics)
		{
			foreach (var nodeAddress in bootstrapNodes)
			{
				if (await TryUpdateMetadataFromBootstrapNode(nodeAddress, topics))
					break;
			}
		}

		private async Task<bool> TryUpdateMetadataFromBootstrapNode(NodeAddress nodeAddress, string[] topics)
		{
			try
			{
				using (var conn = new BrokerConnection(clientId, nodeAddress))
				{
					return await TryUpdateMetadataFromBrokerConnection(topics, conn);
				}
			}
			catch (SocketException)
			{
				return false;
			}
		}
	}
}