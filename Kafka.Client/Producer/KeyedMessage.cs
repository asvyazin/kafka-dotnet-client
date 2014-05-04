namespace Kafka.Client.Producer
{
	public class KeyedMessage<TKey, TValue>
	{
		public KeyedMessage(string topic, TKey key, TValue value)
		{
			Topic = topic;
			Key = key;
			Value = value;
		}

		public string Topic { get; private set; }
		public TKey Key { get; private set; }
		public TValue Value { get; private set; }

		public override string ToString()
		{
			return string.Format("Topic: {0}, Key: {1}, Value: {2}", Topic, Key, Value);
		}
	}
}