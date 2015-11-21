using System;
using System.IO;
using Shuttle.ESB.Core;

namespace Shuttle.ESB.Msmq.Tests
{
    public class MsmqSectionFixture
    {
        protected MsmqSection GetMsmqSection(string file)
        {
            return ShuttleConfigurationSection.Open<MsmqSection>("msmq", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"MsmqSection\files\{0}", file)));
        }
    }
}