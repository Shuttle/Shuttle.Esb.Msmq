using NUnit.Framework;

namespace Shuttle.ESB.Msmq.Tests
{
	[TestFixture]
	public class MsmqSectionTest : MsmqSectionFixture
	{
		[Test]
		[TestCase("Msmq.config")]
		[TestCase("Msmq-Grouped.config")]
		public void Should_be_able_to_load_a_full_configuration(string file)
		{
			var section = GetMsmqSection(file);

			Assert.IsNotNull(section);

			Assert.AreEqual(1500, section.LocalQueueTimeoutMilliseconds);
			Assert.AreEqual(3500, section.RemoteQueueTimeoutMilliseconds);
		}
	}
}