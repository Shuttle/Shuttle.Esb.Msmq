using System;
using System.Messaging;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Msmq
{
    public class MsmqReleaseMessageObserver : 
        IPipelineObserver<OnReleaseMessage>
    {
        public void Execute(OnReleaseMessage pipelineEvent)
        {
            var msmqOptions = pipelineEvent.Pipeline.State.Get<MsmqOptions>();
            var queueTransaction = pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>();
            var queue = pipelineEvent.Pipeline.State.Get<MessageQueue>("queue");
            var journalQueue = pipelineEvent.Pipeline.State.Get<MessageQueue>("journalQueue");

            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));
            Guard.AgainstNull(queueTransaction, nameof(queueTransaction));
            Guard.AgainstNull(queue, nameof(queue));
            Guard.AgainstNull(journalQueue, nameof(journalQueue));

            try
            {
                var journalMessage = ReceiveMessage(pipelineEvent.Pipeline.State.Get<Guid>("messageId"), queueTransaction, journalQueue, msmqOptions.Timeout);

                if (journalMessage == null)
                {
                    return;
                }

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
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.AccessDenied)
                {
                    MsmqQueue.AccessDenied(msmqOptions.Path);
                }

                throw;
            }
        }

        private Message ReceiveMessage(Guid messageId, MessageQueueTransaction tx, MessageQueue journalQueue,
            TimeSpan timeout)
        {
            try
            {
                return journalQueue.ReceiveByCorrelationId($@"{messageId}\1", timeout, tx);
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