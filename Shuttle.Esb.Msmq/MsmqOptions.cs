using System;

namespace Shuttle.Esb.Msmq
{
    public class MsmqOptions
    {
        public const string SectionName = "Shuttle:Msmq";

        public TimeSpan LocalQueueTimeout { get; set; } = TimeSpan.Zero;
        public TimeSpan RemoteQueueTimeout { get; set; } = TimeSpan.FromMilliseconds(2000);
    }
}