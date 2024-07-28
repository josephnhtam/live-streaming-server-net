using k8s;
using k8s.Models;
using LiveStreamingServerNet.KubernetesPod.Utilities.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts
{
    internal interface IKubernetesContext
    {
        IKubernetes KubernetesClient { get; }
        string PodNamespace { get; }
        string PodName { get; }
        Task<V1Pod> GetPodAsync(CancellationToken cancellationToken);
        Task PatchPodAsync(Action<IPodPatcherBuilder> configureBuilder);
        Task WatchPodAsync(Action<WatchEventType, V1Pod> onPodEvent, WatchPodOptions? options = null, CancellationToken stoppingToken = default);
    }

    public record WatchPodOptions(TimeSpan? ReconnectCheck = null, TimeSpan? RetryDelay = null);
}
