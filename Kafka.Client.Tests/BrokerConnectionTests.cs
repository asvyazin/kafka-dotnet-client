using System;
using Kafka.Client.Messages;
using NUnit.Framework;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class BrokerConnectionTests
	{
		private BrokerConnection connection;
		private const int BrokerPort = 9092;
		private const string BrokerHostname = "localhost";
		private const string ClientId = "testClient";

		[SetUp]
		public void Setup()
		{
			connection = new BrokerConnection(ClientId, BrokerHostname, BrokerPort);
			connection.Start();
		}

		[Test]
		public void Metadata_Test()
		{
			var response = (MetadataResponse)connection.SendRequest(new MetadataRequest(new[] { "test" })).Result;
			Assert.Greater(response.Brokers.Length, 0);
		}
	}
}