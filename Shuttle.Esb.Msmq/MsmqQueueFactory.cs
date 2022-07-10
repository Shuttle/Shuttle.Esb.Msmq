using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public class MsmqQueueFactory : IQueueFactory
    {
        private readonly IOptions<MsmqOptions> _msmqOptions;

        public MsmqQueueFactory(IOptions<MsmqOptions> msmqOptions)
        {
            Guard.AgainstNull(msmqOptions, nameof(msmqOptions));

            _msmqOptions = msmqOptions;
        }

        public string Scheme => MsmqUriParser.Scheme;

        public IQueue Create(Uri uri)
        {
            Guard.AgainstNull(uri, "uri");

            return new MsmqQueue(uri, _msmqOptions);
        }
    }
}