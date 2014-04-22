using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.Produce
{
	public class ProduceRequest : RequestMessage
	{
		private const Int16 Version = 0;
		public ProduceRequest(Int16 requiredAcks, Int32 timeout, ProduceRequestTopicItem[] topicItems)
			: base(ApiKey.ProduceRequest, Version)
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