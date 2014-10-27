using NUnit.Framework;
using Shuttle.ESB.Tests;

namespace Shuttle.ESB.Msmq.Tests
{
	public class MsmqDeferredMessageTest : DeferredFixture
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_be_able_to_perform_full_processing(bool isTransactionalEndpoint)
		{
			TestDeferredProcessing(@"msmq://./{0}", isTransactionalEndpoint);
		}
	}
}