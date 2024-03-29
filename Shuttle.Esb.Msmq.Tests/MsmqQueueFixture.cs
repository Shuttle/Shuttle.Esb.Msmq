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
            TestUnacknowledgedMessage(MsmqFixture.GetServiceCollection(), "msmq://local/{0}");
        }

        [Test]
        public void Should_be_able_to_perform_simple_enqueue_and_get_message()
        {
            TestSimpleEnqueueAndGetMessage(MsmqFixture.GetServiceCollection(), "msmq://local/{0}");
        }

        [Test]
        public void Should_be_able_to_release_a_message()
        {
            TestReleaseMessage(MsmqFixture.GetServiceCollection(), "msmq://local/{0}");
        }
    }
}