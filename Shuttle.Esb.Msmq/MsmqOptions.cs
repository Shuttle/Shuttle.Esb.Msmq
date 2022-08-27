using System;

namespace Shuttle.Esb.Msmq
{
    public class MsmqOptions
    {
        public const string SectionName = "Shuttle:ServiceBus:Msmq";

        public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
        public string Path { get; set; }
        public bool UseDeadLetterQueue { get; set; } = true;
    }
}