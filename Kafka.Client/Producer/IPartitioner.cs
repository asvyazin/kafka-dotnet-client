namespace Kafka.Client.Producer
{
	public interface IPartitioner<in TKey>
	{
		int Partition(TKey key, int numOfPartitions);
	}
}