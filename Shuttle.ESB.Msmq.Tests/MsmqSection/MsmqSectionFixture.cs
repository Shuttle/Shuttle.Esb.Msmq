namespace Shuttle.ESB.Msmq.Tests
{
	public class MsmqSectionFixture
	{
		protected MsmqSection GetMsmqSection(string file)
		{
			return MsmqSection.Open(string.Format(@".\MsmqSection\files\{0}", file));
		}
	}
}