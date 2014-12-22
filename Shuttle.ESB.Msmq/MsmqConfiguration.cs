using System.Configuration;

namespace Shuttle.ESB.Msmq
{
	public class MsmqConfiguration : IMsmqConfiguration
	{
		public MsmqConfiguration()
		{
			LocalQueueTimeoutMilliseconds = 0;
			RemoteQueueTimeoutMilliseconds = 2000;
		}

		public int LocalQueueTimeoutMilliseconds { get; set; }
		public int RemoteQueueTimeoutMilliseconds { get; set; }
	}
}