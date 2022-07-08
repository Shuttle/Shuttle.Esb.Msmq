using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shuttle.Core.Configuration;

namespace Shuttle.Esb.Msmq.Tests
{
    [TestFixture]
    public class MsmqSettingsFixture
    {
        protected MsmqSettings GetSettings()
        {
            var result = new MsmqSettings();

            new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\appsettings.json")).Build()
                .GetRequiredSection($"{MsmqSettings.SectionName}").Bind(result);

            return result;
        }

        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var settings = GetSettings();

            Assert.IsNotNull(settings);

            Assert.AreEqual(TimeSpan.Zero, settings.LocalQueueTimeout);
            Assert.AreEqual(TimeSpan.FromSeconds(2), settings.RemoteQueueTimeout);
        }
    }
}