using System;
using System.Messaging;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Msmq
{
    public class MsmqReturnJournalObserver :
        IPipelineObserver<OnReturnJournalMessages>,
        IPipelineObserver<OnStart>
    {
        private readonly ILog _log;
        private readonly MessagePropertyFilter _messagePropertyFilter;

        public MsmqReturnJournalObserver()
        {
            _log = Log.For(this);

            _messagePropertyFilter = new MessagePropertyFilter();
            _messagePropertyFilter.SetAll();
        }

        public void Execute(OnReturnJournalMessages pipelineEvent)
        {
            var parser = pipelineEvent.Pipeline.State.Get<MsmqUriParser>();
            var tx = pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>();
            var queue = pipelineEvent.Pipeline.State.Get<MessageQueue>("queue");
            var journalQueue = pipelineEvent.Pipeline.State.Get<MessageQueue>("journalQueue");
            var timeout = pipelineEvent.Pipeline.State.Get<TimeSpan>("timeout");
            var done = false;

            try
            {
                while (!done)
                {
                    var journalMessage = DequeueJournalMessage(tx, journalQueue, timeout);

                    if (journalMessage != null)
                    {
                        var message = new Message
                        {
                            Recoverable = true,
                            UseDeadLetterQueue = parser.UseDeadLetterQueue,
                            Label = journalMessage.Label,
                            CorrelationId = string.Format(@"{0}\1", journalMessage.Label),
                            BodyStream = journalMessage.BodyStream.Copy()
                        };

                        queue.Send(message, tx);
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
                    MsmqQueue.AccessDenied(_log, parser.Path);
                }

                _log.Error(string.Format(Resources.GetMessageError, parser.Path, ex.Message));

                throw;
            }
        }

        public void Execute(OnStart pipelineEvent)
        {
            var parser = pipelineEvent.Pipeline.State.Get<MsmqUriParser>();

            pipelineEvent.Pipeline.State.Add("queue", new MessageQueue(parser.Path)
            {
                MessageReadPropertyFilter = _messagePropertyFilter
            });

            pipelineEvent.Pipeline.State.Add("journalQueue", new MessageQueue(parser.JournalPath)
            {
                MessageReadPropertyFilter = _messagePropertyFilter
            });
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
                    MsmqQueue.AccessDenied(_log, journalQueue.Path);
                }

                _log.Error(string.Format(Resources.GetMessageError, journalQueue.Path, ex.Message));

                throw;
            }
        }
    }
}