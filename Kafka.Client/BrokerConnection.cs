using System;
using System.Threading.Tasks;
using Kafka.Client.Messages;
using Kafka.Client.RawProtocol;
using Kafka.Client.Utils;

namespace Kafka.Client
{
	public class BrokerConnection: IDisposable
	{
		private readonly string clientId;
		private readonly BrokerRawConnection brokerRawConnection;

		private volatile int currentCorrelationId;

		public BrokerConnection(string clientId, string host, int port)
		{
			this.clientId = clientId;
			brokerRawConnection = new BrokerRawConnection(host, port);
		}

		public async Task StartAsync()
		{
			await brokerRawConnection.StartAsync();
		}

		public async Task<ResponseMessage> SendRequestAsync(RequestMessage request)
		{
			var rawRequest = ToRawRequest(request);
			var rawResponse = await brokerRawConnection.SendRawRequestAsync(rawRequest);
			return ResponseMessage.FromBytes(request.ApiKey, rawResponse.ResponseData);
		}

		private RawRequest ToRawRequest(RequestMessage request)
		{
			return new RawRequest
			{
				ApiKey = request.ApiKey,
				ApiVersion = request.ApiVersion,
				ClientId = clientId,
				CorrelationId = GetCorrelationId(),
				RequestData = request.ToBytes(),
			};
		}

		private int GetCorrelationId()
		{
			return ++currentCorrelationId;
		}

		public void Dispose()
		{
			if (brokerRawConnection != null)
				brokerRawConnection.Dispose();
		}
	}
}