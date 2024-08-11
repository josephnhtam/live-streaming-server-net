using LiveStreamingServerNet.KubernetesPod.StreamRegistration;
using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts
{
    internal interface IStreamRegistry
    {
        Task<StreamRegistrationResult> RegisterStreamAsync(ISessionInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task UnregsiterStreamAsync(string streamPath);
        Task<bool> IsStreamRegisteredAsync(string streamPath, bool checkLocalOnly = false);
    }
}
