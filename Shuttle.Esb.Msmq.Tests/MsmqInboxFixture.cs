using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Msmq.Tests
{
	public class MsmqInboxFixture : InboxFixture
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_be_able_handle_errors(bool isTransactionalEndpoint)
		{
			TestInboxError(MsmqFixture.GetComponentContainer(), "msmq://./{0}", isTransactionalEndpoint);
		}

		[Test]
		[TestCase(300, false)]
		[TestCase(300, true)]
		public void Should_be_able_to_process_messages_concurrently(int msToComplete, bool isTransactionalEndpoint)
		{
			TestInboxConcurrency(MsmqFixture.GetComponentContainer(), "msmq://./{0}", msToComplete, isTransactionalEndpoint);
		}

		[Test]
		[TestCase(300, false)]
		[TestCase(300, true)]
		public void Should_be_able_to_process_queue_timeously(int count, bool isTransactionalEndpoint)
		{
			TestInboxThroughput(MsmqFixture.GetComponentContainer(), "msmq://./{0}", 1000, count, isTransactionalEndpoint);
		}

		[Test]
		public void Should_be_able_to_handle_a_deferred_message()
		{
			TestInboxDeferred(MsmqFixture.GetComponentContainer(), "msmq://./{0}");
		}

		[Test]
		public void Should_be_able_to_expire_a_message()
		{
			TestInboxExpiry(MsmqFixture.GetComponentContainer(), "msmq://./{0}");
		}
	}
}