using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Fetch
{
	public class FetchRequest: RequestMessage
	{
		private readonly int replicaId;
		private readonly int maxWaitTime;
		private readonly int minBytes;
		private readonly FetchRequestTopicItem[] topicItems;
		private const Int16 Version = 0;

		public FetchRequest(int replicaId, int maxWaitTime, int minBytes, FetchRequestTopicItem[] topicItems)
			: base(ApiKey.FetchRequest, Version)
		{
			this.replicaId = replicaId;
			this.maxWaitTime = maxWaitTime;
			this.minBytes = minBytes;
			this.topicItems = topicItems;
		}

		public override void Write(Stream stream)
		{
			stream.WriteInt32(replicaId);
			stream.WriteInt32(maxWaitTime);
			stream.WriteInt32(minBytes);
			stream.WriteArray(topicItems);
		}
	}
}