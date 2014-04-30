using System;
using System.Threading.Tasks;
using Kafka.Client.Connection;
using Kafka.Client.Metadata;
using Kafka.Client.Metadata.Store;
using Kafka.Client.Producer;
using NUnit.Framework;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class KafkaProducerTests
	{
		private const string TopicName = "test";
		private const string Host = "localhost";
		private const int Port = 9092;
		private const string ClientId = "testClient";

		class GuidEncoder : IEncoder<Guid>
		{
			public byte[] Encode(Guid value)
			{
				return value.ToByteArray();
			}
		}

		class Partitioner : IPartitioner<Guid>
		{
			public int Partition(Guid key, int numOfPartitions)
			{
				return key.GetHashCode()%numOfPartitions;
			}
		}

		[Test]
		public async Task Test_Producer()
		{
			var encoder = new GuidEncoder();
			var brokerConnectionManager = new BrokerConnectionManager(ClientId);
			var metadataStore = new MetadataStore();
			var metadataManagerSettings = new MetadataManagerSettings
			{
				ClientId = ClientId,
				BootstrapNodes = new []{new NodeAddress(Host, Port)},
			};
			var metadataManager = new MetadataManager(metadataStore, brokerConnectionManager, metadataManagerSettings);
			var producerSettings = new KafkaProducerSettings
			{
				RequiredAcks = 0,
				SendRetryCount = 5,
				Timeout = 5,
			};
			var producer = new KafkaProducer<Guid, Guid>(encoder, encoder, new Partitioner(), brokerConnectionManager, metadataManager, producerSettings);

			const int batchesCount = 10;
			const int batchSize = 10;

			for (var i = 0; i < batchesCount; ++i)
			{
				var messages = new KeyedMessage<Guid, Guid>[batchSize];
				for (var j = 0; j < batchSize; ++j)
				{
					messages[j] = new KeyedMessage<Guid, Guid>(TopicName, Guid.NewGuid(), Guid.NewGuid());
				}
				await producer.SendMessagesAsync(messages);
			}
		}
	}
}