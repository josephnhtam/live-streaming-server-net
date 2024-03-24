namespace LiveStreamingServerNet.KubernetesPod.Services.Contracts
{
    public interface IPodStatus
    {
        int StreamsCount { get; }
        int StreamsLimit { get; }
        bool IsPendingStop { get; }
        bool IsStreamsLimitReached { get; }
    }
}
