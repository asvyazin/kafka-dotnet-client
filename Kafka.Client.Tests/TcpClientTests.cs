using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kafka.Client.Utils;
using NUnit.Framework;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class TcpClientTests
	{
		private const int Port = 12345;
		private const int Count = 100000;

		[Test]
		public void Test_ClientsRace()
		{
			var tcpListener = new TcpListener(IPAddress.Any, Port);
			var server = RunServer(tcpListener, 2 * Count);
			var tcpClient = new TcpClient("localhost", Port);
			var clientStream = tcpClient.GetStream();
			var client1 = RunClient(clientStream, 0, Count);
			var client2 = RunClient(clientStream, Count, 2*Count);
			Task.WaitAll(server, client1, client2);
		}

		private static async Task RunClient(Stream clientStream, int from, int to)
		{
			// BAD
//			for (var i = from; i < to; ++i)
//			{
//				Console.WriteLine("writing {0}", i);
//				await clientStream.WriteByteAsync((byte) i);
//			}
			// GOOD
			var bytes = new byte[to - from];
			for (var i = from; i < to; ++i)
			{
				bytes[i - from] = (byte) i;
			}
			await clientStream.WriteAsync(bytes, 0, to - from);
		}

		private static async Task RunServer(TcpListener tcpListener, int count)
		{
			tcpListener.Start();
			var serverConnection = await tcpListener.AcceptTcpClientAsync();
			var serverStream = serverConnection.GetStream();
			for (var i = 0; i < count; ++i)
			{
				var b = await serverStream.ReadByteAsync();
				Assert.AreEqual((byte)i, b);
			}
		}
	}
}