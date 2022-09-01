using System;
using System.Diagnostics;
using System.IO;
using System.Messaging;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Msmq
{
    public class MsmqQueue : IQueue, ICreateQueue, IDropQueue, IPurgeQueue
    {
        private readonly ReusableObjectPool<MsmqGetMessagePipeline> _dequeuePipelinePool;
        private readonly string _path;
        private readonly string _journalPath;

        private readonly MessagePropertyFilter _messagePropertyFilter;
        private readonly TimeSpan _millisecondTimeSpan = TimeSpan.FromMilliseconds(1);
        private readonly Type _msmqDequeuePipelineType = typeof(MsmqGetMessagePipeline);
        private readonly MsmqOptions _msmqOptions;
        private readonly object _lock = new object();
        private bool _journalInitialized;

        public MsmqQueue(QueueUri uri, MsmqOptions msmqOptions)
        {
            Guard.AgainstNull(uri, nameof(uri));
            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));

            _msmqOptions = msmqOptions;

            Uri = uri;

            _path = $"{msmqOptions.Path}{(msmqOptions.Path.EndsWith("\\") ? string.Empty : "\\")}{Uri.QueueName}";
            _journalPath = string.Concat(_path, "$journal");

            _messagePropertyFilter = new MessagePropertyFilter();
            _messagePropertyFilter.SetAll();

            _dequeuePipelinePool = new ReusableObjectPool<MsmqGetMessagePipeline>();
        }

        public void Create()
        {
            CreateJournal();

            if (Exists())
            {
                return;
            }

            MessageQueue.Create(_path, true).Dispose();
        }

        public void Drop()
        {
            DropJournal();

            if (!Exists())
            {
                return;
            }

            MessageQueue.Delete(_path);
        }

        public void Purge()
        {
            using (var queue = CreateQueue())
            {
                queue.Purge();
            }
        }

        public QueueUri Uri { get; }
        public bool IsStream => false;

        public bool IsEmpty()
        {
            try
            {
                using (var queue = CreateQueue())
                {
                    return queue.Peek(_msmqOptions.Timeout) == null;
                }
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    return true;
                }

                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.AccessDenied)
                {
                    AccessDenied(_path);
                }

                throw;
            }
        }

        public void Enqueue(TransportMessage transportMessage, Stream stream)
        {
            var timeToBeReceived = transportMessage.ExpiryDate - DateTime.Now;

            if (transportMessage.HasExpired() || timeToBeReceived < _millisecondTimeSpan)
            {
                return;
            }

            var sendMessage = new Message
            {
                Recoverable = true,
                UseDeadLetterQueue = _msmqOptions.UseDeadLetterQueue,
                Label = transportMessage.MessageId.ToString(),
                CorrelationId = $@"{transportMessage.MessageId}\1",
                BodyStream = stream
            };

            if (transportMessage.HasExpiryDate())
            {
                sendMessage.TimeToBeReceived = timeToBeReceived;
            }

            if (transportMessage.HasPriority())
            {
                var priority = (MessagePriority)transportMessage.Priority;

                if (priority < MessagePriority.Lowest)
                {
                    priority = MessagePriority.Lowest;
                }

                if (priority > MessagePriority.Highest)
                {
                    priority = MessagePriority.Highest;
                }

                sendMessage.Priority = priority;
            }

            try
            {
                using (var queue = CreateQueue())
                {
                    queue.Send(sendMessage, MessageQueueTransactionType.Single);
                }
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.AccessDenied)
                {
                    AccessDenied(_path);
                }

                throw;
            }
        }

        public ReceivedMessage GetMessage()
        {
            if (!_journalInitialized)
            {
                CreateJournal();
                ReturnJournalMessages();
            }

            var pipeline = _dequeuePipelinePool.Get(_msmqDequeuePipelineType) ?? new MsmqGetMessagePipeline();

            pipeline.Execute(_msmqOptions, CreateQueue(), CreateJournalQueue());

            _dequeuePipelinePool.Release(pipeline);

            var message = pipeline.State.Get<Message>();

            return message == null ? null : new ReceivedMessage(message.BodyStream, new Guid(message.Label));
        }

        public void Acknowledge(object acknowledgementToken)
        {
            var messageId = (Guid)acknowledgementToken;

            try
            {
                lock (_lock)
                {
                    using (var queue = CreateJournalQueue())
                    {
                        queue.ReceiveByCorrelationId($@"{messageId}\1", MessageQueueTransactionType.Single);
                    }
                }
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.AccessDenied)
                {
                    AccessDenied(_path);
                }

                throw;
            }
        }

        public void Release(object acknowledgementToken)
        {
            if (!Exists()
                ||
                !JournalExists())
            {
                return;
            }

            new MsmqReleaseMessagePipeline().Execute((Guid)acknowledgementToken, _msmqOptions, CreateQueue(), CreateJournalQueue());
        }

        private void ReturnJournalMessages()
        {
            lock (_lock)
            {
                if (_journalInitialized
                    ||
                    !Exists()
                    ||
                    !JournalExists())
                {
                    return;
                }

                new MsmqReturnJournalPipeline().Execute(_msmqOptions, CreateQueue(), CreateJournalQueue());

                _journalInitialized = true;
            }
        }

        private void CreateJournal()
        {
            if (JournalExists())
            {
                return;
            }

            MessageQueue.Create(_journalPath, true).Dispose();
        }

        private void DropJournal()
        {
            if (!JournalExists())
            {
                return;
            }

            MessageQueue.Delete(_journalPath);
        }

        private bool Exists()
        {
            return MessageQueue.Exists(_path);
        }

        private bool JournalExists()
        {
            return MessageQueue.Exists(_journalPath);
        }

        private MessageQueue CreateQueue()
        {
            return new MessageQueue(_path)
            {
                MessageReadPropertyFilter = _messagePropertyFilter
            };
        }

        private MessageQueue CreateJournalQueue()
        {
            return new MessageQueue(_journalPath)
            {
                MessageReadPropertyFilter = _messagePropertyFilter
            };
        }

        public static void AccessDenied(string path)
        {
            Guard.AgainstNullOrEmptyString(path, nameof(path));

            if (Environment.UserInteractive)
            {
                return;
            }

            Process.GetCurrentProcess().Kill();
        }
    }
}