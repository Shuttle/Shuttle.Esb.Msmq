using System;
using System.Messaging;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Msmq
{
    public class MsmqReleaseMessagePipeline : Pipeline
    {
        public MsmqReleaseMessagePipeline()
        {
            RegisterStage("Release")
                .WithEvent<OnStart>()
                .WithEvent<OnBeginTransaction>()
                .WithEvent<OnReleaseMessage>()
                .WithEvent<OnCommitTransaction>()
                .WithEvent<OnDispose>();

            RegisterObserver(new MsmqTransactionObserver());
            RegisterObserver(new MsmqReleaseMessageObserver());
        }

        public bool Execute(Guid messageId, MsmqOptions msmqOptions, MessageQueue queue, MessageQueue journalQueue)
        {
            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));
            Guard.AgainstNull(queue, nameof(queue));
            Guard.AgainstNull(journalQueue, nameof(journalQueue));

            State.Clear();

            State.Add("messageId", messageId);
            State.Add(msmqOptions);
            State.Add("queue", queue);
            State.Add("journalQueue", journalQueue);

            return base.Execute();
        }
    }
}