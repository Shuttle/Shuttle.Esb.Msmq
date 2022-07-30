using System;
using System.Messaging;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Msmq
{
    public class MsmqGetMessagePipeline : Pipeline
    {
        public MsmqGetMessagePipeline()
        {
            RegisterStage("Dequeue")
                .WithEvent<OnStart>()
                .WithEvent<OnBeginTransaction>()
                .WithEvent<OnReceiveMessage>()
                .WithEvent<OnSendJournalMessage>()
                .WithEvent<OnCommitTransaction>()
                .WithEvent<OnDispose>();

            RegisterObserver(new MsmqTransactionObserver());
            RegisterObserver(new MsmqGetMessageObserver());
        }

        public bool Execute(MsmqOptions msmqOptions, MessageQueue queue, MessageQueue journalQueue)
        {
            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));
            Guard.AgainstNull(queue, nameof(queue));
            Guard.AgainstNull(journalQueue, nameof(journalQueue));

            State.Clear();

            State.Add(msmqOptions);
            State.Add("queue", queue);
            State.Add("journalQueue", journalQueue);

            return base.Execute();
        }
    }
}