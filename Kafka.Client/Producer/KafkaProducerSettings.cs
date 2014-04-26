using Kafka.Client.Metadata;

namespace Kafka.Client.Producer
{
	public class KafkaProducerSettings
	{
		public short RequiredAcks { get; set; }
		public int Timeout { get; set; }
		public int SendRetryCount { get; set; }
		public NodeAddress[] BootstrapNodes { get; set; }
		public string ClientId { get; set; }
	}
}