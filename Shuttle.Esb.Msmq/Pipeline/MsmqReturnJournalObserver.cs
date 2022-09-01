using System;
using System.Messaging;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Msmq
{
    public class MsmqReturnJournalObserver : 
        IPipelineObserver<OnReturnJournalMessages>
    {
        public void Execute(OnReturnJournalMessages pipelineEvent)
        {
            var msmqOptions = pipelineEvent.Pipeline.State.Get<MsmqOptions>();
            var queueTransaction = pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>();
            var queue = pipelineEvent.Pipeline.State.Get<MessageQueue>("queue");
            var journalQueue = pipelineEvent.Pipeline.State.Get<MessageQueue>("journalQueue");
            var done = false;

            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));
            Guard.AgainstNull(queueTransaction, nameof(queueTransaction));
            Guard.AgainstNull(queue, nameof(queue));
            Guard.AgainstNull(journalQueue, nameof(journalQueue));

            try
            {
                while (!done)
                {
                    var journalMessage = DequeueJournalMessage(queueTransaction, journalQueue, msmqOptions.Timeout);

                    if (journalMessage != null)
                    {
                        var message = new Message
                        {
                            Recoverable = true,
                            UseDeadLetterQueue = msmqOptions.UseDeadLetterQueue,
                            Label = journalMessage.Label,
                            CorrelationId = $@"{journalMessage.Label}\1",
                            BodyStream = journalMessage.BodyStream.Copy()
                        };

                        queue.Send(message, queueTransaction);
                    }
                    else
                    {
                        done = true;
                    }
                }
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.AccessDenied)
                {
                    MsmqQueue.AccessDenied(msmqOptions.Path);
                }

                throw;
            }
        }

        private Message DequeueJournalMessage(MessageQueueTransaction tx, MessageQueue journalQueue, TimeSpan timeout)
        {
            try
            {
                return journalQueue.Receive(timeout, tx);
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    return null;
                }

                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.AccessDenied)
                {
                    MsmqQueue.AccessDenied(journalQueue.Path);
                }

                throw;
            }
        }
    }
}