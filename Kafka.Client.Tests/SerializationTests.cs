using System;
using System.IO;
using Kafka.Client.Messages;
using NUnit.Framework;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class SerializationTests
	{
		[Test]
		public void Test_EmptyMetadataRequest()
		{
			var req = new MetadataRequest();
			var bytes = SerializeRequestMessage(req);
			Assert.AreEqual(2 * sizeof(Int32), bytes.Length);
		}

		[Test]
		public void Test_MetadataRequest()
		{
			const string s1 = "123";
			const string s2 = "456";
			var req = new MetadataRequest(new[] {s1, s2});
			var bytes = SerializeRequestMessage(req);
			Assert.AreEqual(4 * sizeof(Int32) + s1.Length + s2.Length, bytes.Length);
		}

		private static byte[] SerializeRequestMessage(RequestMessage message)
		{
			using (var stream = new MemoryStream())
			{
				message.Write(stream);
				return stream.ToArray();
			}
		}
	}
}