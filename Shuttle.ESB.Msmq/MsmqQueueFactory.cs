using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Msmq
{
	public class MsmqQueueFactory : IQueueFactory
	{
		public IMsmqConfiguration Configuration { get; private set; }

		public MsmqQueueFactory(IMsmqConfiguration configuration)
		{
			Configuration = configuration;
		}

		public string Scheme
		{
			get { return MsmqUriParser.Scheme; }
		}

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