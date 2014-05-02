using System.Text;
using NUnit.Framework;
using P4N;

namespace Kafka.Client.Tests
{
	[TestFixture]
	public class Crc32Tests
	{
		[Test]
		public void Crc32()
		{
			const string str = "123456789";
			var crc = CRC32.CalcArray(Encoding.UTF8.GetBytes(str));
			Assert.AreEqual(0xCBF43926U, crc);
		}
	}
}