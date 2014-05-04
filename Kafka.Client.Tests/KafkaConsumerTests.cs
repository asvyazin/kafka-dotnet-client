using System;
using System.Diagnostics;
using System.Threading;
using Kafka.Client.Connection;
using Kafka.Client.Consumer;
using Kafka.Client.Metadata;
using Kafka.Client.Metadata.Store;
using NUnit.Framework;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class KafkaConsumerTests
	{
		private const string TopicName = "test";
		private const string Host = "localhost";
		private const int Port = 9092;
		private const string ClientId = "testClient";

		class Decoder : IDecoder<Guid>
		{
			public Guid Decode(byte[] bytes)
			{
				return new Guid(bytes);
			}
		}

		[Test]
		public void Test_Consumer()
		{
			var decoder = new Decoder();
			var brokerConnectionManager = new BrokerConnectionManager(ClientId);
			var metadataManagerSettings = new MetadataManagerSettings
			{
				ClientId = ClientId,
				BootstrapNodes = new []{new NodeAddress(Host, Port)},
			};
			var metadataManager = new MetadataManager(new MetadataStore(), brokerConnectionManager, metadataManagerSettings);
			var settings = new KafkaConsumerSettings
			{
				MinBytes = 1,
				MaxBytes = 4096,
				MaxWaitTime = 10000,
				SendRetryCount = 5,
			};
			var consumer = new KafkaConsumer<Guid, Guid>(decoder, decoder, brokerConnectionManager, metadataManager, settings);

			var sw = Stopwatch.StartNew();
			var sw1 = Stopwatch.StartNew();
			var previousOffset = 0L;
			var ev = new ManualResetEvent(false);
			consumer.ConsumeMessages(TopicName, 0, 0).Subscribe(messageAndOffset =>
			{
				if (messageAndOffset.Offset - previousOffset >= 100000)
				{
					Console.WriteLine("{0}, {1} / {2}", messageAndOffset.Offset, sw.ElapsedMilliseconds, sw1.ElapsedMilliseconds);
					sw.Restart();
					previousOffset = messageAndOffset.Offset;
				}
			});
			Assert.True(ev.WaitOne());
		}
	}
}