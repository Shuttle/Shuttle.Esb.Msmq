using System.Configuration;
using Shuttle.Core.Infrastructure;
using Shuttle.ESB.Core;

namespace Shuttle.ESB.Msmq
{
	public class MsmqSection : ConfigurationSection
	{
		[ConfigurationProperty("localQueueTimeoutMilliseconds", IsRequired = false, DefaultValue = 0)]
		public int LocalQueueTimeoutMilliseconds
		{
			get
			{
				return (int)this["localQueueTimeoutMilliseconds"];
			}
		}

		[ConfigurationProperty("remoteQueueTimeoutMilliseconds", IsRequired = false, DefaultValue = 2000)]
		public int RemoteQueueTimeoutMilliseconds
		{
			get
			{
				return (int)this["remoteQueueTimeoutMilliseconds"];
			}
		}

		public static MsmqConfiguration Configuration()
		{
			var section = ConfigurationSectionProvider.Open<MsmqSection>("shuttle", "msmq");
			var configuration = new MsmqConfiguration();

			if (section != null)
			{
				configuration.LocalQueueTimeoutMilliseconds = section.LocalQueueTimeoutMilliseconds;
				configuration.RemoteQueueTimeoutMilliseconds = section.RemoteQueueTimeoutMilliseconds;
			}

			return configuration;
		}
	}
}