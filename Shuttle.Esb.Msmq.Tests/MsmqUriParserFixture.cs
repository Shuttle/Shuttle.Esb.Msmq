using System;
using NUnit.Framework;

namespace Shuttle.Esb.Msmq.Tests
{
    [TestFixture]
    public class MsmqUriParserFixture
    {
        [Test]
        public void Should_be_able_to_parse_all_parameters()
        {
            const string osPath = @"FormatName:DIRECT=OS:the-host\private$\the-queue";
            const string tcpPath = @"FormatName:DIRECT=TCP:10.100.1.2\private$\the-queue";
            const string dotPath = @".\private$\the-queue";
            var localPath = $@"{Environment.MachineName.ToLowerInvariant()}\private$\the-queue";

            var parser = new MsmqUriParser(new Uri("msmq://the-host/the-queue"));

            Assert.IsFalse(parser.Local);
            Assert.AreEqual(osPath, parser.Path);
            Assert.AreEqual(string.Concat(osPath, "$journal"), parser.JournalPath);
            Assert.IsTrue(parser.UseDeadLetterQueue);

            parser = new MsmqUriParser(new Uri("msmq://10.100.1.2/the-queue?useDeadLetterQueue=false"));

            Assert.IsFalse(parser.Local);
            Assert.AreEqual(tcpPath, parser.Path);
            Assert.AreEqual(string.Concat(tcpPath, "$journal"), parser.JournalPath);
            Assert.IsFalse(parser.UseDeadLetterQueue);

            parser = new MsmqUriParser(new Uri("msmq://./the-queue?useDeadLetterQueue=false"));

            Assert.IsTrue(parser.Local);
            Assert.AreEqual(dotPath, parser.Path);
            Assert.AreEqual(string.Concat(dotPath, "$journal"), parser.JournalPath);
            Assert.IsFalse(parser.UseDeadLetterQueue);

            parser =
                new MsmqUriParser(new Uri($"msmq://{Environment.MachineName}/the-queue?useDeadLetterQueue=true"));

            Assert.IsTrue(parser.Local);
            Assert.AreEqual(localPath, parser.Path);
            Assert.AreEqual(string.Concat(localPath, "$journal"), parser.JournalPath);
            Assert.IsTrue(parser.UseDeadLetterQueue);
        }
    }
}