using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Produce
{
	public class ProduceRequest : RequestMessage
	{
		public ProduceRequest(Int16 requiredAcks, Int32 timeout, ProduceRequestTopicItem[] topicItems): base(ApiKey.ProduceRequest)
		{
			this.topicItems = topicItems;
			this.timeout = timeout;
			this.requiredAcks = requiredAcks;
		}

		public override void Write(Stream stream)
		{
			stream.WriteInt16(requiredAcks);
			stream.WriteInt32(timeout);
			stream.WriteArray(topicItems);
		}

		private readonly ProduceRequestTopicItem[] topicItems;

		private readonly Int16 requiredAcks;

		private readonly Int32 timeout;
	}
}