using System;
using System.IO;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Msmq.Tests
{
	public class MsmqSectionFixture
	{
		protected MsmqSection GetMsmqSection(string file)
		{
			return ConfigurationSectionProvider.OpenFile<MsmqSection>("shuttle", "msmq",
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"MsmqSection\files\{0}", file)));
		}
	}
}