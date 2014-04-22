using System.IO;

namespace Kafka.Client.Utils
{
	public interface IWriteable
	{
		void Write(Stream stream);
	}
}