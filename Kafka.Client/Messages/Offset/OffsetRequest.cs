using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Offset
{
	public class OffsetRequest: RequestMessage
	{
		public OffsetRequest(Int32 replicaId, OffsetRequestTopicItem[] topicItems) : base(ApiKey.OffsetRequest)
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