using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages
{
	public class MetadataRequest : RequestMessage
	{
		private readonly string[] topics;

		public MetadataRequest(string[] topics = null)
			: base(ApiKey.MetadataRequest)
		{
			this.topics = topics ?? new string[0];
		}

		protected override int GetBytesCount()
		{
			return BytesHelper.GetArrayBytesCount(topics);
		}

		protected override void WriteMessage(Stream stream)
		{
			stream.WriteArray(topics, (s, str) => s.WriteString(str));
		}
	}
}