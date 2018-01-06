using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public class MsmqQueueFactory : IQueueFactory
    {
        public MsmqQueueFactory(IMsmqConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IMsmqConfiguration Configuration { get; }

        public string Scheme => MsmqUriParser.Scheme;

        public IQueue Create(Uri uri)
        {
            Guard.AgainstNull(uri, "uri");

            return new MsmqQueue(uri, Configuration);
        }

        public bool CanCreate(Uri uri)
        {
            Guard.AgainstNull(uri, "uri");

            return Scheme.Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}