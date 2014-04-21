using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Kafka.Client.Utils;

namespace Kafka.Client.RawProtocol
{
	public class BrokerRawConnection: IDisposable
	{
		private readonly NetworkStream clientStream;

		private readonly ConcurrentDictionary<Int32, TaskCompletionSource<RawResponse>> waitingRequests =
			new ConcurrentDictionary<int, TaskCompletionSource<RawResponse>>();

		public BrokerRawConnection(TcpClient tcpClient)
		{
			clientStream = tcpClient.GetStream();
		}

		public void Dispose()
		{
			if (clientStream != null)
				clientStream.Dispose();
		}

		public async Task<RawResponse> SendRequestAsync(RawRequest request)
		{
			var tcs = new TaskCompletionSource<RawResponse>();
			waitingRequests[request.CorrelationId] = tcs;
			using (var ms = new MemoryStream())
			{
				request.Write(ms);
				await ms.CopyToAsync(clientStream);
			}
			return await tcs.Task;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await ReceiveRawResponse(cancellationToken);
			}
		}

		private async Task ReceiveRawResponse(CancellationToken cancellationToken)
		{
			var bytes = await clientStream.ReadBytesAsync(cancellationToken);
			using (var ms = new MemoryStream(bytes))
			{
				var response = RawResponse.Read(ms);
				var tcs = waitingRequests[response.CorrelationId];
				tcs.SetResult(response);
			}
		}
	}
}