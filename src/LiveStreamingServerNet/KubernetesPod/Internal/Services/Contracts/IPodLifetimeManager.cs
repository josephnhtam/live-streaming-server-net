using LiveStreamingServerNet.KubernetesPod.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts
{
    internal interface IPodLifetimeManager : IPodStatus, IRtmpServerConnectionEventHandler, IRtmpServerStreamEventHandler
    {
        ValueTask ReconcileAsync(IDictionary<string, string> labels, IDictionary<string, string> annotations);
    }
}
