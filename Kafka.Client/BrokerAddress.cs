namespace Kafka.Client
{
	public class BrokerAddress
	{
		public int NodeId { get; set; }
		public NodeAddress Endpoint { get; set; }
	}
}