using System;
using System.Diagnostics;
using System.IO;
using System.Messaging;
using System.Security.Principal;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb.Msmq
{
    public class MsmqQueue : IQueue, ICreateQueue, IDropQueue, IPurgeQueue
    {
        private readonly ReusableObjectPool<MsmqGetMessagePipeline> _dequeuePipelinePool;

        private readonly ILog _log;
        private readonly MessagePropertyFilter _messagePropertyFilter;
        private readonly TimeSpan _millisecondTimeSpan = TimeSpan.FromMilliseconds(1);
        private readonly Type _msmqDequeuePipelineType = typeof(MsmqGetMessagePipeline);
        private readonly object _padlock = new object();
        private readonly MsmqUriParser _parser;
        private readonly TimeSpan _timeout;
        private bool _journalInitialized;

        public MsmqQueue(Uri uri, IMsmqConfiguration configuration)
        {
            Guard.AgainstNull(uri, "uri");
            Guard.AgainstNull(configuration, "configuration");

            _log = Log.For(this);

            _parser = new MsmqUriParser(uri);

            _timeout = _parser.Local
                ? TimeSpan.FromMilliseconds(configuration.LocalQueueTimeoutMilliseconds)
                : TimeSpan.FromMilliseconds(configuration.RemoteQueueTimeoutMilliseconds);

            Uri = _parser.Uri;

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

            if (!_parser.Local)
            {
                throw new InvalidOperationException(string.Format(Resources.CannotCreateRemoteQueue, Uri));
            }

            MessageQueue.Create(_parser.Path, true).Dispose();

            _log.Information(string.Format(Resources.QueueCreated, _parser.Path));
        }

        public void Drop()
        {
            DropJournal();

            if (!Exists())
            {
                return;
            }

            if (!_parser.Local)
            {
                throw new InvalidOperationException(string.Format(Resources.CannotDropRemoteQueue, Uri));
            }

            MessageQueue.Delete(_parser.Path);

            _log.Information(string.Format(Resources.QueueDropped, _parser.Path));
        }

        public void Purge()
        {
            using (var queue = CreateQueue())
            {
                queue.Purge();
            }

            _log.Information(string.Format(Resources.QueuePurged, _parser.Path));
        }

        public Uri Uri { get; }

        public bool IsEmpty()
        {
            try
            {
                using (var queue = CreateQueue())
                {
                    return queue.Peek(_timeout) == null;
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
                    AccessDenied(_log, _parser.Path);
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
                UseDeadLetterQueue = _parser.UseDeadLetterQueue,
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
                var priority = (MessagePriority) transportMessage.Priority;

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
                    AccessDenied(_log, _parser.Path);
                }

                _log.Error(string.Format(Resources.SendMessageIdError, transportMessage.MessageId, Uri,
                    ex.AllMessages()));

                throw;
            }
            catch (Exception ex)
            {
                _log.Error(string.Format(Resources.SendMessageIdError, transportMessage.MessageId, Uri,
                    ex.AllMessages()));

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

            try
            {
                var pipeline = _dequeuePipelinePool.Get(_msmqDequeuePipelineType) ?? new MsmqGetMessagePipeline();

                pipeline.Execute(_parser, _timeout);

                _dequeuePipelinePool.Release(pipeline);

                var message = pipeline.State.Get<Message>();

                return message == null ? null : new ReceivedMessage(message.BodyStream, new Guid(message.Label));
            }
            catch (Exception ex)
            {
                _log.Error(string.Format(Resources.GetMessageError, _parser.Path, ex.AllMessages()));

                throw;
            }
        }

        public void Acknowledge(object acknowledgementToken)
        {
            var messageId = (Guid) acknowledgementToken;

            try
            {
                lock (_padlock)
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
                    AccessDenied(_log, _parser.Path);
                }

                _log.Error(string.Format(Resources.RemoveError, messageId, _parser.Path, ex.AllMessages()));

                throw;
            }
            catch (Exception ex)
            {
                _log.Error(string.Format(Resources.RemoveError, messageId, _parser.Path, ex.AllMessages()));

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

            new MsmqReleaseMessagePipeline().Execute((Guid) acknowledgementToken, _parser, _timeout);
        }

        private void ReturnJournalMessages()
        {
            lock (_padlock)
            {
                if (_journalInitialized
                    ||
                    !Exists()
                    ||
                    !JournalExists())
                {
                    return;
                }

                new MsmqReturnJournalPipeline().Execute(_parser, _timeout);

                _journalInitialized = true;
            }
        }

        private void CreateJournal()
        {
            if (JournalExists())
            {
                return;
            }

            if (!_parser.Local)
            {
                throw new InvalidOperationException(string.Format(Resources.CannotCreateRemoteQueue, Uri));
            }

            MessageQueue.Create(_parser.JournalPath, true).Dispose();

            _log.Information(string.Format(Resources.QueueCreated, _parser.Path));
        }

        private void DropJournal()
        {
            if (!JournalExists())
            {
                return;
            }

            if (!_parser.Local)
            {
                throw new InvalidOperationException(string.Format(Resources.CannotDropRemoteQueue, Uri));
            }

            MessageQueue.Delete(_parser.JournalPath);

            _log.Information(string.Format(Resources.QueueDropped, _parser.JournalPath));
        }

        private bool Exists()
        {
            return MessageQueue.Exists(_parser.Path);
        }

        private bool JournalExists()
        {
            return MessageQueue.Exists(_parser.JournalPath);
        }

        private MessageQueue CreateQueue()
        {
            return new MessageQueue(_parser.Path)
            {
                MessageReadPropertyFilter = _messagePropertyFilter
            };
        }

        private MessageQueue CreateJournalQueue()
        {
            return new MessageQueue(_parser.JournalPath)
            {
                MessageReadPropertyFilter = _messagePropertyFilter
            };
        }

        public static void AccessDenied(ILog log, string path)
        {
            Guard.AgainstNull(log, "log");
            Guard.AgainstNull(path, "path");

            log.Fatal(
                string.Format(
                    Resources.AccessDenied,
                    WindowsIdentity.GetCurrent().Name,
                    path));

            if (Environment.UserInteractive)
            {
                return;
            }

            Process.GetCurrentProcess().Kill();
        }
    }
}