using System.Messaging;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Msmq
{
    public class MsmqTransactionObserver :
        IPipelineObserver<OnStart>,
        IPipelineObserver<OnBeginTransaction>,
        IPipelineObserver<OnCommitTransaction>,
        IPipelineObserver<OnDispose>
    {
        public void Execute(OnBeginTransaction pipelineEvent)
        {
            var queueTransaction = pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>();

            if (queueTransaction == null)
            {
                return;
            }

            queueTransaction.Begin();
        }

        public void Execute(OnCommitTransaction pipelineEvent)
        {
            var queueTransaction = pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>();

            if (queueTransaction == null)
            {
                return;
            }

            queueTransaction.Commit();
        }

        public void Execute(OnDispose pipelineEvent)
        {
            var queueTransaction = pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>();

            if (queueTransaction == null)
            {
                return;
            }

            queueTransaction.Dispose();
            pipelineEvent.Pipeline.State.Replace<MessageQueueTransaction>(null);
        }

        public void Execute(OnStart pipelineEvent)
        {
            pipelineEvent.Pipeline.State.Add(new MessageQueueTransaction());
        }
    }
}