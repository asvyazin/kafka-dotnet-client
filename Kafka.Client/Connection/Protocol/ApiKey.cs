namespace Kafka.Client.Connection.Protocol
{
	public enum ApiKey : ushort
	{
		ProduceRequest = 0,
		FetchRequest = 1,
		OffsetRequest = 2,
		MetadataRequest = 3,
		LeaderAndIsrRequest = 4,
		StopReplicaRequest = 5,
		OffsetCommitRequest = 8,
		OffsetFetchRequest = 9
	}
}