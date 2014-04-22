using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Metadata
{
	public class MetadataRequest : RequestMessage
	{
		private readonly string[] topics;

		public MetadataRequest(string[] topics = null)
			: base(ApiKey.MetadataRequest)
		{
			this.topics = topics ?? new string[0];
		}

		public override void Write(Stream stream)
		{
			stream.WriteArray(topics, (s, i) => s.WriteString(i));
		}
	}
}