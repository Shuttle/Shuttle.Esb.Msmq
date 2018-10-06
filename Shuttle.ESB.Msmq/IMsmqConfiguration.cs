namespace Shuttle.Esb.Msmq
{
    public interface IMsmqConfiguration
    {
        int LocalQueueTimeoutMilliseconds { get; set; }
        int RemoteQueueTimeoutMilliseconds { get; set; }
    }
}