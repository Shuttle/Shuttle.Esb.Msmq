using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Msmq.Tests
{
	public class MsmqDistributorFixture : DistributorFixture
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_be_able_to_distribute_messages(bool isTransactionalEndpoint)
		{
			TestDistributor(MsmqFixture.GetComponentContainer(), MsmqFixture.GetComponentContainer(), @"msmq://./{0}", isTransactionalEndpoint);
		}
	}
}