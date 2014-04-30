using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kafka.Client.Connection.Raw.Protocol;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Raw
{
	public class BrokerRawConnection: IDisposable
	{
		private bool disposed;
		private readonly object locker = new object();
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
			lock (locker)
			{
				if (disposed)
					return;

				if (clientStream != null)
					clientStream.Dispose();
				if (tcpClient != null)
					tcpClient.Close();
				disposed = true;
			}
		}

		public async Task<RawResponse> SendRawRequestAsync(RawRequest request)
		{
			ThrowObjectDisposedExceptionIfNeeded();
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

		private void ThrowObjectDisposedExceptionIfNeeded()
		{
			lock (locker)
			{
				if (disposed)
					throw new ObjectDisposedException(typeof(BrokerRawConnection).Name);
			}
		}

		public async Task StartAsync()
		{
			ThrowObjectDisposedExceptionIfNeeded();
			while (!disposed)
			{
				await ReceiveRawResponse();
			}
		}

		private async Task ReceiveRawResponse()
		{
			byte[] bytes;
			try
			{
				bytes = await clientStream.ReadBytesAsync();
			}
			catch (Exception ex)
			{
				if (!disposed)
					Dispose();

				while (!waitingRequests.IsEmpty)
				{
					var request = waitingRequests.FirstOrDefault();

					TaskCompletionSource<RawResponse> tcs;
					if (waitingRequests.TryRemove(request.Key, out tcs))
						tcs.SetException(ex);
				}

				return;
			}

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