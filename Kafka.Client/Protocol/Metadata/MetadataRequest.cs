using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.Metadata
{
	public class MetadataRequest : RequestMessage
	{
		private readonly string[] topics;
		private const Int16 Version = 0;

		public MetadataRequest(string[] topics = null)
			: base(ApiKey.MetadataRequest, Version)
		{
			this.topics = topics ?? new string[0];
		}

		public override void Write(Stream stream)
		{
			stream.WriteArray(topics, (s, i) => s.WriteString(i));
		}
	}
}