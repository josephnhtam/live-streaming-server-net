using LiveStreamingServerNet.KubernetesPod.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts
{
    internal interface IPodLifetimeManager : IPodStatus
    {
        ValueTask ReconcileAsync(IDictionary<string, string> labels, IDictionary<string, string> annotations);

        ValueTask OnClientDisposedAsync(uint clientId);
        ValueTask OnStreamPublishedAsync(uint clientId, string streamIdentifier);
        ValueTask OnStreamUnpublishedAsync(uint clientId, string streamIdentifier);
    }
}
