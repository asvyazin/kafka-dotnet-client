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
		private readonly TcpClient tcpClient;
		private readonly NetworkStream clientStream;

		private volatile int currentCorrelationId;

		public BrokerConnection(string clientId, string hostname, int port)
		{
			this.clientId = clientId;
			tcpClient = new TcpClient(hostname, port);
			clientStream = tcpClient.GetStream();
		}

		public async Task<ResponseMessage> SendRequest(RequestMessage request)
		{
			TaskCompletionSource<ResponseMessage> tcs;
			var correlationId = RegisterWaitingRequest(out tcs, request.ApiKey);

			var memoryStream = new MemoryStream();

			memoryStream.WriteInt16((Int16)request.ApiKey);
			memoryStream.WriteInt16(ApiVersion);
			memoryStream.WriteInt32(correlationId);
			memoryStream.WriteString(clientId);
			request.Write(memoryStream);

			await memoryStream.CopyToAsync(clientStream);

			return await tcs.Task;
		}

		public async Task Start()
		{
			while (true)
			{
				await ReceiveResponseMessageAsync();
			}
		}

		private async Task ReceiveResponseMessageAsync()
		{
			var responseMessageBytes = await clientStream.ReadBytesAsync();
			using (var memoryStream = new MemoryStream(responseMessageBytes))
			{
				var correlationId = memoryStream.ReadInt32();
				var requestWaiting = RemoveWaitingRequest(correlationId);
				var responseMessage = DeserializeResponseMessage(requestWaiting.ApiKey, memoryStream);
				requestWaiting.TaskCompletionSource.SetResult(responseMessage);
			}
		}

		private ResponseMessage DeserializeResponseMessage(ApiKey apiKey, Stream stream)
		{
			switch (apiKey)
			{
				case ApiKey.ProduceRequest:
					break;
				case ApiKey.FetchRequest:
					break;
				case ApiKey.OffsetRequest:
					break;
				case ApiKey.MetadataRequest:
					return MetadataResponse.FromStream(stream);
				case ApiKey.LeaderAndIsrRequest:
					break;
				case ApiKey.StopReplicaRequest:
					break;
				case ApiKey.OffsetCommitRequest:
					break;
				case ApiKey.OffsetFetchRequest:
					break;
				default:
					throw new ArgumentOutOfRangeException("apiKey");
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
			clientStream.Dispose();
			tcpClient.Close();
		}
	}
}