using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kafka.Client.Connection.Raw;
using Kafka.Client.Utils;
using NUnit.Framework;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class BrokerRawConnectionTests
	{
		private const int Port = 12345;
		private readonly NodeAddress nodeAddress = new NodeAddress("localhost", Port);

		[Test]
		public void ConnectionStartStop()
		{
			var tcpListener = new TcpListener(IPAddress.Any, Port);
			var serverTask = RunServer(tcpListener);

			var conn = new BrokerRawConnection(nodeAddress);
			var clientTask = conn.StartAsync(CancellationToken.None);
			Thread.Sleep(TimeSpan.FromSeconds(1));
			conn.Dispose();

			var ex = Assert.Throws<AggregateException>(() => Task.WaitAll(new[] {serverTask, clientTask}, TimeSpan.FromSeconds(10)));
			Assert.That(ex.InnerExceptions.Count, Is.EqualTo(2));
		}

		private static async Task RunServer(TcpListener tcpListener)
		{
			tcpListener.Start();
			var serverConnection = await tcpListener.AcceptTcpClientAsync();
			var serverStream = serverConnection.GetStream();
			Console.WriteLine("accepted client connection");
			await serverStream.ReadByteAsync(CancellationToken.None);
		}
	}
}