namespace Kafka.Client.Consumer
{
	public class KafkaConsumerSettings
	{
		public int MaxWaitTime { get; set; }
		public int MinBytes { get; set; }
		public int MaxBytes { get; set; }
		public int SendRetryCount { get; set; }
	}
}