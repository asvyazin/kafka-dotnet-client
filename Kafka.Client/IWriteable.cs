using System.IO;

namespace Kafka.Client
{
	public interface IWriteable
	{
		void Write(Stream stream);
	}
}