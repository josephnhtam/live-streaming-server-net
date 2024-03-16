using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts
{
    internal interface IPodLifetimeManager : IRtmpServerConnectionEventHandler, IRtmpServerStreamEventHandler
    {
        int StreamCount { get; }
        bool IsPendingStop { get; }
        ValueTask ReconcileAsync(IDictionary<string, string> labels, IDictionary<string, string> annotations);
    }
}
