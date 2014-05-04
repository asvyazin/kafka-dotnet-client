using System;
using System.Linq;

namespace Kafka.Client.Producer
{
	public class SendMessagesFailedException<TKey, TValue> : Exception
	{
		public KeyedMessage<TKey, TValue>[] NotSentMessages { get; private set; }

		public SendMessagesFailedException(KeyedMessage<TKey, TValue>[] notSentMessages)
		{
			NotSentMessages = notSentMessages;
		}

		public override string ToString()
		{
			return string.Format("{0}, NotSentMessages: [{1}]", base.ToString(), string.Join(", ", NotSentMessages.Select(m => m.ToString()).ToArray()));
		}
	}
}