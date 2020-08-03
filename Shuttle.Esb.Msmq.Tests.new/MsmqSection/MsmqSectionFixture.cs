using System;
using System.IO;
using NUnit.Framework;
using Shuttle.Core.Configuration;

namespace Shuttle.Esb.Msmq.Tests
{
    [TestFixture]
    public class MsmqSectionFixture
    {
        protected MsmqSection GetMsmqSection(string file)
        {
            return ConfigurationSectionProvider.OpenFile<MsmqSection>("shuttle", "msmq",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"MsmqSection\files\{file}"));
        }

        [Test]
        [TestCase("Msmq.config")]
        [TestCase("Msmq-Grouped.config")]
        public void Should_be_able_to_load_a_full_configuration(string file)
        {
            var section = GetMsmqSection(file);

            Assert.IsNotNull(section);

            Assert.AreEqual(1500, section.LocalQueueTimeoutMilliseconds);
            Assert.AreEqual(3500, section.RemoteQueueTimeoutMilliseconds);
        }
    }
}