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
		private readonly Dictionary<int, RequestWaiting> requestsWaiting = new Dictionary<int, RequestWaiting>();

		private const Int16 ApiVersion = 0;
		private readonly string clientId;
		private TcpClient tcpClient;
		private NetworkStream clientStream;

		private volatile int currentCorrelationId;

		public BrokerConnection(string clientId)
		{
			this.clientId = clientId;
		}

		public async Task ConnectAsync(string hostname, int port)
		{
			tcpClient = new TcpClient();
			await tcpClient.ConnectAsync(hostname, port);
			clientStream = tcpClient.GetStream();
		}

		public async Task<ResponseMessage> SendRequestAsync(RequestMessage request)
		{
			TaskCompletionSource<ResponseMessage> tcs;
			var correlationId = RegisterWaitingRequest(out tcs, request.ApiKey);

			byte[] bytes;
			using (var memoryStream = new MemoryStream())
			{
				memoryStream.WriteInt16((Int16)request.ApiKey);
				memoryStream.WriteInt16(ApiVersion);
				memoryStream.WriteInt32(correlationId);
				memoryStream.WriteString(clientId);
				request.Write(memoryStream);
				bytes = memoryStream.ToArray();
			}

			await clientStream.WriteBytesAsync(bytes);
			return await tcs.Task;
		}

		public async Task StartAsync()
		{
			while (true)
			{
				await ReceiveResponseMessageAsync();
			}
		}

		private async Task ReceiveResponseMessageAsync()
		{
			var responseMessageBytes = await clientStream.ReadBytesAsync();
			RequestWaiting requestWaiting = null;

			try
			{
				ResponseMessage responseMessage;
				using (var memoryStream = new MemoryStream(responseMessageBytes))
				{
					var correlationId = memoryStream.ReadInt32();
					requestWaiting = RemoveWaitingRequest(correlationId);
					responseMessage = ResponseMessage.FromStream(requestWaiting.ApiKey, memoryStream);
				}
				requestWaiting.TaskCompletionSource.SetResult(responseMessage);
			}
			catch (Exception ex)
			{
				if (requestWaiting != null)
					requestWaiting.TaskCompletionSource.SetException(ex);
				else
					throw;
			}
		}

		private RequestWaiting RemoveWaitingRequest(int correlationId)
		{
			lock (locker)
			{
				var request = requestsWaiting[correlationId];
				requestsWaiting.Remove(correlationId);
				return request;
			}
		}

		private int RegisterWaitingRequest(out TaskCompletionSource<ResponseMessage> tcs, ApiKey apiKey)
		{
			lock (locker)
			{
				var correlationId = GetCorrelationId();
				tcs = new TaskCompletionSource<ResponseMessage>();
				var requestWaiting = new RequestWaiting
				{
					TaskCompletionSource = tcs,
					ApiKey = apiKey,
				};
				requestsWaiting.Add(correlationId, requestWaiting);
				return correlationId;
			}
		}

		private int GetCorrelationId()
		{
			return ++currentCorrelationId;
		}

		public void Dispose()
		{
			if (clientStream != null)
				clientStream.Dispose();
			if (tcpClient != null)
				tcpClient.Close();
		}
	}
}