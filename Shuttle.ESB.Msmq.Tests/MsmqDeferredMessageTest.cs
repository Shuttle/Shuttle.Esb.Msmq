using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Msmq.Tests
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