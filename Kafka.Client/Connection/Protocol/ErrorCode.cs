namespace Kafka.Client.Connection.Protocol
{
	public enum ErrorCode: short
	{
		NoError = 0,
		Unknown = -1,
		OffsetOutOfRange = 1,
		InvalidMessage = 2,
		UnknownTopicOrPartition = 3,
		InvalidMessageSize = 4,
		LeaderNotAvailable = 5,
		NotLeaderForPartition = 6,
		RequestTimedOut = 7,
		BrokerNotAvailable = 8,
		MessageSizeTooLarge = 10,
		StaleControllerEpochCode = 11,
		OffsetMetadataTooLargeCode = 12
	}
}