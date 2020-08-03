using System;
using System.Messaging;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Msmq
{
    public class MsmqGetMessageObserver :
        IPipelineObserver<OnStart>,
        IPipelineObserver<OnReceiveMessage>,
        IPipelineObserver<OnSendJournalMessage>,
        IPipelineObserver<OnDispose>
    {
        private readonly ILog _log;
        private readonly MessagePropertyFilter _messagePropertyFilter;

        public MsmqGetMessageObserver()
        {
            _messagePropertyFilter = new MessagePropertyFilter();
            _messagePropertyFilter.SetAll();

            _log = Log.For(this);
        }

        public void Execute(OnDispose pipelineEvent)
        {
            var queue = pipelineEvent.Pipeline.State.Get<MessageQueue>("queue");

            queue?.Dispose();

            var journalQueue = pipelineEvent.Pipeline.State.Get<MessageQueue>("journalQueue");

            journalQueue?.Dispose();
        }

        public void Execute(OnReceiveMessage pipelineEvent)
        {
            var parser = pipelineEvent.Pipeline.State.Get<MsmqUriParser>();
            var tx = pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>();

            try
            {
                pipelineEvent.Pipeline.State.Add(
                    pipelineEvent.Pipeline.State.Get<MessageQueue>("queue")
                        .Receive(pipelineEvent.Pipeline.State.Get<TimeSpan>("timeout"), tx));
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
                    MsmqQueue.AccessDenied(_log, parser.Path);
                }

                _log.Error(string.Format(Resources.GetMessageError, parser.Uri, ex.Message));

                throw;
            }
        }

        public void Execute(OnSendJournalMessage pipelineEvent)
        {
            var parser = pipelineEvent.Pipeline.State.Get<MsmqUriParser>();
            var journalQueue = pipelineEvent.Pipeline.State.Get<MessageQueue>("journalQueue");
            var message = pipelineEvent.Pipeline.State.Get<Message>();

            if (journalQueue == null || message == null)
            {
                return;
            }

            var journalMessage = new Message
            {
                Recoverable = true,
                UseDeadLetterQueue = parser.UseDeadLetterQueue,
                Label = message.Label,
                CorrelationId = $@"{message.Label}\1",
                BodyStream = message.BodyStream.Copy()
            };

            journalQueue.Send(journalMessage, pipelineEvent.Pipeline.State.Get<MessageQueueTransaction>());
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
    }
}