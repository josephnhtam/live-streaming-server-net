using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.StreamRegistration.Contracts
{
    public interface IStreamStore
    {
        Task<StreamRegistrationResult> RegisterStreamAsync(IClientInfo client, string podNamespace, string podName, string streamPath, IReadOnlyDictionary<string, string> streamArguments, CancellationToken cancellationToken = default);
        Task UnregsiterStreamAsync(string streamPath, CancellationToken cancellationToken = default);
        Task<bool> IsStreamRegisteredAsync(string streamPath, CancellationToken cancellationToken = default);
        Task<StreamRevalidationResult> RevalidateStreamAsync(string streamPath, CancellationToken cancellationToken = default);
    }
}
