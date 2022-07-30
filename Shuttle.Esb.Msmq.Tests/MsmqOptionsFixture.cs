using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Shuttle.Esb.Msmq.Tests
{
    [TestFixture]
    public class MsmqOptionsFixture
    {
        protected MsmqOptions GetOptions()
        {
            var result = new MsmqOptions();

            new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\appsettings.json")).Build()
                .GetRequiredSection($"{MsmqOptions.SectionName}").Bind(result);

            return result;
        }

        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var settings = GetOptions();

            Assert.IsNotNull(settings);

            Assert.AreEqual(TimeSpan.Zero, settings.Timeout);
        }
    }
}