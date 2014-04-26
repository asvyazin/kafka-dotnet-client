namespace Kafka.Client.Producer
{
	public interface IEncoder<in T>
	{
		byte[] Encode(T value);
	}
}