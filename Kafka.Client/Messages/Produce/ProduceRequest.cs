using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Messages.Produce
{
	public class ProduceRequest : RequestMessage
	{
		public ProduceRequest(Int16 requiredAcks, Int32 timeout, ProduceRequestTopicItem[] topicItems): base(ApiKey.ProduceRequest)
		{
			TopicItems = topicItems;
			Timeout = timeout;
			RequiredAcks = requiredAcks;
		}

		public override void WriteMessage(Stream stream)
		{
			stream.WriteInt16(RequiredAcks);
			stream.WriteInt32(Timeout);
			stream.WriteArray(TopicItems, (s, i) => i.Write(stream));
		}

		public ProduceRequestTopicItem[] TopicItems { get; private set; }

		public Int16 RequiredAcks { get; private set; }

		public Int32 Timeout { get; private set; }
	}
}