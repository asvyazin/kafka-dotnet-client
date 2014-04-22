using System;
using System.IO;
using Kafka.Client.Utils;

namespace Kafka.Client.Protocol.OffsetFetch
{
	public class OffsetFetchRequest: RequestMessage
	{
		private readonly string consumerGroup;
		private readonly OffsetFetchRequestTopicItem[] topicItems;

		private const Int16 Version = 0;
		public OffsetFetchRequest(string consumerGroup, OffsetFetchRequestTopicItem[] topicItems)
			: base(ApiKey.OffsetFetchRequest, Version)
		{
			this.consumerGroup = consumerGroup;
			this.topicItems = topicItems;
		}

		public override void Write(Stream stream)
		{
			stream.WriteString(consumerGroup);
			stream.WriteArray(topicItems);
		}
	}
}