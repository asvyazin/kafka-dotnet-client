using System;

namespace Kafka.Client.Producer
{
	public class SendMessagesFailedException<TKey, TValue> : Exception
	{
		public KeyedMessage<TKey, TValue>[] NotSentMessages { get; private set; }

		public SendMessagesFailedException(KeyedMessage<TKey, TValue>[] notSentMessages)
		{
			NotSentMessages = notSentMessages;
		}
	}
}