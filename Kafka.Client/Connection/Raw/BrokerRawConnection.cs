using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kafka.Client.Connection.Raw.Protocol;
using Kafka.Client.Metadata;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Raw
{
	public class BrokerRawConnection: IDisposable
	{
		private readonly TcpClient tcpClient;
		private readonly NetworkStream clientStream;

		private readonly ConcurrentDictionary<Int32, TaskCompletionSource<RawResponse>> waitingRequests =
			new ConcurrentDictionary<int, TaskCompletionSource<RawResponse>>();

		public BrokerRawConnection(NodeAddress nodeAddress)
		{
			tcpClient = new TcpClient(nodeAddress.Host, nodeAddress.Port);
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
			byte[] bytes;
			using (var ms = new MemoryStream())
			{
				ms.WriteBytes(request.ToBytes());
				bytes = ms.ToArray();
			}
			await clientStream.WriteAsync(bytes, 0, bytes.Length);
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
				TaskCompletionSource<RawResponse> tcs;
				if (!waitingRequests.TryRemove(response.CorrelationId, out tcs))
					throw new InvalidOperationException("Could not remove waiting request from collection");
				tcs.SetResult(response);
			}
		}
	}
}