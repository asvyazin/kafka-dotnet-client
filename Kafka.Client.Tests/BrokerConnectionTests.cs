using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kafka.Client.Connection;
using Kafka.Client.Connection.Protocol.Metadata;
using NUnit.Framework;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class BrokerConnectionTests
	{
		private BrokerConnection connection;
		private const int BrokerPort = 9092;
		private const string BrokerHostname = "127.0.0.1";
		private static readonly NodeAddress BrokerNodeAddress = new NodeAddress(BrokerHostname, BrokerPort);
		private const string ClientId = "testClient";

		[SetUp]
		public void Setup()
		{
			connection = new BrokerConnection(ClientId, BrokerNodeAddress);
		}

		[Test]
		public async Task Metadata_Test()
		{
			var response = (MetadataResponse)await connection.SendRequestAsync(new MetadataRequest(new[] { "test" }));
			Assert.Greater(response.Brokers.Length, 0);
		}

		[Test]
		public void Metadata_ConcurrentTest()
		{
			const int tasksCount = 100;

			var tasks = new List<Task>();
			for (var i = 0; i < tasksCount; i++)
			{
				tasks.Add(Task.Factory.StartNew(async () =>
				{
					var response = (MetadataResponse)await connection.SendRequestAsync(new MetadataRequest(new[] { "test" }));
					Assert.Greater(response.Brokers.Length, 0);
				}));
			}
			Assert.True(Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(10)));
		}
	}
}