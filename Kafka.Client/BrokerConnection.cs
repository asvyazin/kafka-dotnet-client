using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kafka.Client.Messages;
using Kafka.Client.Utils;

namespace Kafka.Client
{
	public class BrokerConnection: IDisposable
	{
		private readonly object locker = new object();
		private readonly Dictionary<int, TaskCompletionSource<ResponseMessage>> requestsWaiting = new Dictionary<int, TaskCompletionSource<ResponseMessage>>();

		private const Int16 ApiVersion = 0;
		private readonly string clientId;
		private readonly TcpClient tcpClient;
		private readonly NetworkStream clientStream;

		private volatile int currentCorrelationId = 0;

		public BrokerConnection(string clientId, string hostname, int port)
		{
			this.clientId = clientId;
			tcpClient = new TcpClient(hostname, port);
			clientStream = tcpClient.GetStream();
		}

		public async Task<ResponseMessage> SendRequest(RequestMessage request)
		{
			TaskCompletionSource<ResponseMessage> tcs;
			var correlationId = RegisterWaitingRequest(out tcs);

			var memoryStream = new MemoryStream();

			memoryStream.WriteInt16((Int16)request.ApiKey);
			memoryStream.WriteInt16(ApiVersion);
			memoryStream.WriteInt32(correlationId);
			memoryStream.WriteString(clientId);
			request.Write(memoryStream);

			await memoryStream.CopyToAsync(clientStream);

			return await tcs.Task;
		}

		private int RegisterWaitingRequest(out TaskCompletionSource<ResponseMessage> tcs)
		{
			lock (locker)
			{
				var correlationId = GetCorrelationId();
				tcs = new TaskCompletionSource<ResponseMessage>();
				requestsWaiting.Add(correlationId, tcs);
				return correlationId;
			}
		}

		private int GetCorrelationId()
		{
			return ++currentCorrelationId;
		}

		public void Dispose()
		{
			clientStream.Dispose();
			tcpClient.Close();
		}
	}
}