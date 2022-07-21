using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Msmq.Tests
{
    public class MsmqInboxFixture : InboxFixture
    {
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Should_be_able_handle_errors(bool hasErrorQueue, bool isTransactionalEndpoint)
        {
            TestInboxError(MsmqFixture.GetServiceCollection(), "msmq://./{0}", hasErrorQueue, isTransactionalEndpoint);
        }

        [Test]
        [TestCase(300, false)]
        [TestCase(300, true)]
        public void Should_be_able_to_process_messages_concurrently(int msToComplete, bool isTransactionalEndpoint)
        {
            TestInboxConcurrency(MsmqFixture.GetServiceCollection(), "msmq://./{0}", msToComplete,
                isTransactionalEndpoint);
        }

        [Test]
        [TestCase(300, false)]
        [TestCase(300, true)]
        public void Should_be_able_to_process_queue_timeously(int count, bool isTransactionalEndpoint)
        {
            TestInboxThroughput(MsmqFixture.GetServiceCollection(), "msmq://./{0}", 1000, 5, count,
                isTransactionalEndpoint);
        }

        [Test]
        public void Should_be_able_to_handle_a_deferred_message()
        {
            TestInboxDeferred(MsmqFixture.GetServiceCollection(), "msmq://./{0}");
        }

        [Test]
        public void Should_be_able_to_expire_a_message()
        {
            TestInboxExpiry(MsmqFixture.GetServiceCollection(), "msmq://./{0}");
        }
    }
}