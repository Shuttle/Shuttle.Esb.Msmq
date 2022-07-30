using System;
using System.Messaging;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Msmq
{
    public class MsmqGetMessageObserver :
        IPipelineObserver<OnReceiveMessage>,
        IPipelineObserver<OnSendJournalMessage>,
        IPipelineObserver<OnDispose>
    {
        public void Execute(OnDispose pipelineEvent)
        {
            var queue = pipelineEvent.Pipeline.State.Get<MessageQueue>("queue");

            queue?.Dispose();

            var journalQueue = pipelineEvent.Pipeline.State.Get<MessageQueue>("journalQueue");

            journalQueue?.Dispose();
        }

        public void Execute(OnReceiveMessage pipelineEvent)
        {
            var msmqOptions = pipelineEvent.Pipeline.State.Get<MsmqOptions>();
            var queue = pipelineEvent.Pipeline.State.Get<MessageQueue>("queue");
            var queueTransaction = pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>();

            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));
            Guard.AgainstNull(queue, nameof(queue));
            Guard.AgainstNull(queueTransaction, nameof(queueTransaction));

            try
            {
                pipelineEvent.Pipeline.State.Add(
                    queue
                        .Receive(msmqOptions.Timeout, queueTransaction));
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    pipelineEvent.Pipeline.State.Add<Message>(null);
                    return;
                }

                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.AccessDenied)
                {
                    MsmqQueue.AccessDenied(queue.Path);
                }

                throw;
            }
        }

        public void Execute(OnSendJournalMessage pipelineEvent)
        {
            var msmqOptions = pipelineEvent.Pipeline.State.Get<MsmqOptions>();
            var journalQueue = pipelineEvent.Pipeline.State.Get<MessageQueue>("journalQueue");
            var message = pipelineEvent.Pipeline.State.Get<Message>();

            if (journalQueue == null || message == null)
            {
                return;
            }

            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));

            var journalMessage = new Message
            {
                Recoverable = true,
                UseDeadLetterQueue = msmqOptions.UseDeadLetterQueue,
                Label = message.Label,
                CorrelationId = $@"{message.Label}\1",
                BodyStream = message.BodyStream.Copy()
            };

            journalQueue.Send(journalMessage, pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>());
        }
   }
}