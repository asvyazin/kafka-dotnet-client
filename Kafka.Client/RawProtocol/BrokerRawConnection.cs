using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kafka.Client.Utils;

namespace Kafka.Client.RawProtocol
{
	public class BrokerRawConnection: IDisposable
	{
		private readonly TcpClient tcpClient;
		private readonly NetworkStream clientStream;

		private readonly ConcurrentDictionary<Int32, TaskCompletionSource<RawResponse>> waitingRequests =
			new ConcurrentDictionary<int, TaskCompletionSource<RawResponse>>();

		private const int BufferSize = 81920;

		public BrokerRawConnection(string host, int port)
		{
			tcpClient = new TcpClient(host, port);
			clientStream = tcpClient.GetStream();
		}

		public void Dispose()
		{
			if (clientStream != null)
				clientStream.Dispose();
			if (tcpClient != null)
				tcpClient.Close();
		}

		public async Task<RawResponse> SendRawRequestAsync(RawRequest request)
		{
			var tcs = new TaskCompletionSource<RawResponse>();
			waitingRequests[request.CorrelationId] = tcs;
			using (var ms = new MemoryStream())
			{
				request.Write(ms);
				await ms.CopyToAsync(clientStream, BufferSize);
			}
			return await tcs.Task;
		}

		public async Task StartAsync()
		{
			while (true)
			{
				await ReceiveRawResponse();
			}
		}

		private async Task ReceiveRawResponse()
		{
			var bytes = await clientStream.ReadBytesAsync();
			using (var ms = new MemoryStream(bytes))
			{
				var response = RawResponse.Read(ms);
				var tcs = waitingRequests[response.CorrelationId];
				tcs.SetResult(response);
			}
		}
	}
}