namespace Kafka.Client.Metadata
{
	public class MetadataManagerSettings
	{
		public NodeAddress[] BootstrapNodes { get; set; }
		public string ClientId { get; set; }
	}
}