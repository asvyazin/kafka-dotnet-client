using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Connection.Protocol.Offset
{
	public class OffsetRequest: RequestMessage
	{
		private const Int16 Version = 0;
		public OffsetRequest(Int32 replicaId, OffsetRequestTopicItem[] topicItems)
			: base(ApiKey.OffsetRequest, Version)
		{
			this.topicItems = topicItems;
			this.replicaId = replicaId;
		}

		public override void Write(Stream stream)
		{
			stream.WriteInt32(replicaId);
			stream.WriteArray(topicItems);
		}

		private readonly OffsetRequestTopicItem[] topicItems;

		private readonly Int32 replicaId;
	}
}