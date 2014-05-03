using System;
using System.Threading;
using System.Threading.Tasks;
using Kafka.Client.Connection.Protocol;
using Kafka.Client.Connection.Raw;
using Kafka.Client.Connection.Raw.Protocol;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection
{
	public class BrokerConnection: IDisposable
	{
		private readonly string clientId;
		private readonly BrokerRawConnection brokerRawConnection;

		private volatile int currentCorrelationId;
		private bool disposed;
		private readonly object locker = new object();
		private readonly Task brokerRawConnectionStartedTask;
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		public BrokerConnection(string clientId, NodeAddress nodeAddress)
		{
			this.clientId = clientId;
			brokerRawConnection = new BrokerRawConnection(nodeAddress);
			brokerRawConnectionStartedTask = brokerRawConnection.StartAsync(cancellationTokenSource.Token);
		}

		public async Task<ResponseMessage> SendRequestAsync(RequestMessage request)
		{
			ThrowObjectDisposedExceptionIfNeeded();
			var rawRequest = ToRawRequest(request);
			var rawResponse = await brokerRawConnection.SendRawRequestAsync(rawRequest);
			return ResponseMessage.FromBytes(request.ApiKey, rawResponse.ResponseData);
		}

		private void ThrowObjectDisposedExceptionIfNeeded()
		{
			lock (locker)
			{
				if (disposed)
					throw new ObjectDisposedException(typeof (BrokerConnection).Name);
			}
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
			lock (locker)
			{
				if (disposed)
					return;

				cancellationTokenSource.Cancel();
				brokerRawConnection.Dispose();
				disposed = true;
			}
		}

		public bool IsDisposed()
		{
			lock (locker)
			{
				return disposed;
			}
		}
	}
}