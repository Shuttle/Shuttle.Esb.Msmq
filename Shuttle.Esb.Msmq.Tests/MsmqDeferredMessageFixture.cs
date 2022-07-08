using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Msmq.Tests
{
    public class MsmqDeferredMessageFixture : DeferredFixture
    {
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Should_be_able_to_perform_full_processing(bool isTransactionalEndpoint)
        {
            TestDeferredProcessing(MsmqFixture.GetServiceCollection(), @"msmq://./{0}", isTransactionalEndpoint);
        }
    }
}