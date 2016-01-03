using System;
using System.IO;
using Shuttle.Core.Infrastructure;
using Shuttle.ESB.Core;

namespace Shuttle.ESB.Msmq.Tests
{
    public class MsmqSectionFixture
    {
        protected MsmqSection GetMsmqSection(string file)
        {
            return ConfigurationSectionProvider.OpenFile<MsmqSection>("shuttle", "msmq", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"MsmqSection\files\{0}", file)));
        }
    }
}