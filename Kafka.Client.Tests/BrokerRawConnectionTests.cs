using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kafka.Client.RawProtocol;
using Kafka.Client.Utils;
using NUnit.Framework;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class BrokerRawConnectionTests
	{
		private const int Port = 12345;

		[Test]
		public void ConnectionStartStop()
		{
			var cts = new CancellationTokenSource();

			var tcpListener = new TcpListener(IPAddress.Any, Port);
			var serverTask = RunServer(tcpListener, cts.Token);

			using (var tcpClient = new TcpClient("localhost", Port))
			using (var conn = new BrokerRawConnection(tcpClient))
			{
				var clientTask = conn.StartAsync(cts.Token);
				cts.CancelAfter(TimeSpan.FromSeconds(1));
				var ex = Assert.Throws<AggregateException>(() => Task.WaitAll(new[] {serverTask, clientTask}, TimeSpan.FromSeconds(10)));
				Assert.That(ex.InnerExceptions.Count, Is.EqualTo(2));
				Assert.That(ex.InnerExceptions, Is.All.InstanceOf<TaskCanceledException>());
			}
		}

		private static async Task RunServer(TcpListener tcpListener, CancellationToken cancellationToken)
		{
			tcpListener.Start();
			var serverConnection = await tcpListener.AcceptTcpClientAsync();
			var serverStream = serverConnection.GetStream();
			Console.WriteLine("accepted client connection");
			await serverStream.ReadByteAsync(cancellationToken);
		}
	}
}