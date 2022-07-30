using System;
using System.Messaging;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Msmq
{
    public class MsmqReturnJournalPipeline : Pipeline
    {
        public MsmqReturnJournalPipeline()
        {
            RegisterStage("Return")
                .WithEvent<OnStart>()
                .WithEvent<OnBeginTransaction>()
                .WithEvent<OnReturnJournalMessages>()
                .WithEvent<OnCommitTransaction>()
                .WithEvent<OnDispose>();

            RegisterObserver(new MsmqTransactionObserver());
            RegisterObserver(new MsmqReturnJournalObserver());
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