using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Msmq.Tests
{
    public class MsmqPipelineExceptionHandlingFixture : PipelineExceptionFixture
    {
        [Test]
        public void Should_be_able_to_handle_exceptions_in_receive_stage_of_receive_pipeline()
        {
            TestExceptionHandling(MsmqFixture.GetComponentContainer(), "msmq://./{0}");
        }
    }
}