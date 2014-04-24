using System;

namespace Kafka.Client.Interface
{
	public class BatchProduceRequest
	{
		public BatchProduceRequest(Int32 timeout, Int16 requiredAcks, ProduceMessageItem[] messages)
		{
			Timeout = timeout;
			RequiredAcks = requiredAcks;
			Messages = messages;
		}

		public Int32 Timeout { get; private set; }
		public Int16 RequiredAcks { get; private set; }
		public ProduceMessageItem[] Messages { get; private set; }
	}
}