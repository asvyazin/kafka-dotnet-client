using Kafka.Client.Connection.Protocol;

namespace Kafka.Client.Producer
{
	public class KeyedMessageAndError<TKey, TValue>
	{
		public KeyedMessage<TKey, TValue> Message { get; set; }
		public ErrorCode Error { get; set; }

		public override string ToString()
		{
			return string.Format("Message: {0}, Error: {1}", Message, Error);
		}
	}
}