using LiveStreamingServerNet.KubernetesPod.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts
{
    internal interface IPodLifetimeManager : IPodStatus
    {
        ValueTask ReconcileAsync(IReadOnlyDictionary<string, string> labels, IReadOnlyDictionary<string, string> annotations);

        ValueTask OnClientDisposedAsync(uint clientId);
        ValueTask OnStreamPublishedAsync(uint clientId, string streamIdentifier);
        ValueTask OnStreamUnpublishedAsync(uint clientId, string streamIdentifier);
    }
}
