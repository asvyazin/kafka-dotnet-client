﻿using System;
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

			byte[] bytes;
			using (var memoryStream = new MemoryStream())
			{
				memoryStream.WriteInt16((Int16)request.ApiKey);
				memoryStream.WriteInt16(ApiVersion);
				memoryStream.WriteInt32(correlationId);
				memoryStream.WriteString(clientId);
				request.WriteMessage(memoryStream);
				bytes = memoryStream.ToArray();
			}

			await clientStream.WriteBytesAsync(bytes);
			return await tcs.Task;
		}

		public async void Start()
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
					responseMessage = DeserializeResponseMessage(requestWaiting.ApiKey, memoryStream);
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

		private ResponseMessage DeserializeResponseMessage(ApiKey apiKey, Stream stream)
		{
			switch (apiKey)
			{
				case ApiKey.ProduceRequest:
					throw new NotImplementedException();
				case ApiKey.FetchRequest:
					throw new NotImplementedException();
				case ApiKey.OffsetRequest:
					throw new NotImplementedException();
				case ApiKey.MetadataRequest:
					return MetadataResponse.FromStream(stream);
				case ApiKey.LeaderAndIsrRequest:
					throw new NotImplementedException();
				case ApiKey.StopReplicaRequest:
					throw new NotImplementedException();
				case ApiKey.OffsetCommitRequest:
					throw new NotImplementedException();
				case ApiKey.OffsetFetchRequest:
					throw new NotImplementedException();
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