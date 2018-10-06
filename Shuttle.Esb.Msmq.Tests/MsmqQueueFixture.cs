using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Msmq.Tests
{
    [TestFixture]
    public class MsmqQueueFixture : BasicQueueFixture
    {
        [Test]
        public void Should_be_able_to_get_message_again_when_not_acknowledged_before_queue_is_disposed()
        {
            TestUnacknowledgedMessage(MsmqFixture.GetComponentContainer(), "msmq://./{0}");
        }

        [Test]
        public void Should_be_able_to_perform_simple_enqueue_and_get_message()
        {
            TestSimpleEnqueueAndGetMessage(MsmqFixture.GetComponentContainer(), "msmq://./{0}");
        }

        [Test]
        public void Should_be_able_to_release_a_message()
        {
            TestReleaseMessage(MsmqFixture.GetComponentContainer(), "msmq://./{0}");
        }
    }
}