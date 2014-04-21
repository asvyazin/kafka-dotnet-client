using System.Threading.Tasks;
using Kafka.Client.Messages.Metadata;
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
			connection = new BrokerConnection(ClientId);
			connection.ConnectAsync(BrokerHostname, BrokerPort).Wait();
		}

		[Test]
		public async Task Metadata_Test()
		{
			var response = (MetadataResponse)await connection.SendRequestAsync(new MetadataRequest(new[] { "test" }));
			Assert.Greater(response.Brokers.Length, 0);
		}
	}
}