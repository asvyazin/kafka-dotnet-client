using System;
using System.Threading.Tasks;
using Kafka.Client.Metadata;
using Kafka.Client.RawProtocol;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol
{
	public class BrokerConnection: IDisposable
	{
		private readonly string clientId;
		private readonly BrokerRawConnection brokerRawConnection;

		private volatile int currentCorrelationId;

		public BrokerConnection(string clientId, NodeAddress nodeAddress)
		{
			this.clientId = clientId;
			brokerRawConnection = new BrokerRawConnection(nodeAddress);
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