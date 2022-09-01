using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public class MsmqQueueFactory : IQueueFactory
    {
        private readonly IOptionsMonitor<MsmqOptions> _msmqOptions;

        public MsmqQueueFactory(IOptionsMonitor<MsmqOptions> msmqOptions)
        {
            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));

            _msmqOptions = msmqOptions;
        }

        public string Scheme => "msmq";

        public IQueue Create(Uri uri)
        {
            Guard.AgainstNull(uri, "uri");

            var queueUri = new QueueUri(uri).SchemeInvariant(Scheme);
            var msmqOptions = _msmqOptions.Get(queueUri.ConfigurationName);

            if (msmqOptions == null)
            {
                throw new InvalidOperationException(string.Format(Esb.Resources.QueueConfigurationNameException, queueUri.ConfigurationName));
            }

            return new MsmqQueue(queueUri, msmqOptions);
        }
    }
}