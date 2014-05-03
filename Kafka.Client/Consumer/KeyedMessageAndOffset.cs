using System;
using Kafka.Client.Producer;

namespace Kafka.Client.Consumer
{
	public class KeyedMessageAndOffset<TKey, TValue>
	{
		public KeyedMessageAndOffset(long offset, KeyedMessage<TKey, TValue> message)
		{
			Offset = offset;
			Message = message;
		}

		public Int64 Offset { get; private set; }
		public KeyedMessage<TKey, TValue> Message { get; private set; }
	}
}