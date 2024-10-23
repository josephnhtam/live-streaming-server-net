namespace LiveStreamingServerNet.Utilities.Common.Contracts
{
    public interface IRetryCounter
    {
        void Reset();
        bool CanRetry();
        TimeSpan? GetNextBackoff();
    }
}