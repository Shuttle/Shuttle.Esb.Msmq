using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public class MsmqQueueFactory : IQueueFactory
    {
        private readonly MsmqOptions _msmqOptions;

        public MsmqQueueFactory(IOptions<MsmqOptions> msmqOptions)
        {
            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));
            Guard.AgainstNull(msmqOptions.Value, nameof(msmqOptions.Value));

            _msmqOptions = msmqOptions.Value;
        }

        public string Scheme => MsmqUriParser.Scheme;

        public IQueue Create(Uri uri)
        {
            Guard.AgainstNull(uri, "uri");

            return new MsmqQueue(uri, _msmqOptions);
        }
    }
}