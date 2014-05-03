namespace Kafka.Client.Consumer
{
	public interface IDecoder<out T>
	{
		T Decode(byte[] bytes);
	}
}