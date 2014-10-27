using NUnit.Framework;
using Shuttle.ESB.Tests;

namespace Shuttle.ESB.Msmq.Tests
{
	public class MsmqDistributorTest : DistributorFixture
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_be_able_to_distribute_messages(bool isTransactionalEndpoint)
		{
			TestDistributor(@"msmq://./{0}", isTransactionalEndpoint);
		}
	}
}