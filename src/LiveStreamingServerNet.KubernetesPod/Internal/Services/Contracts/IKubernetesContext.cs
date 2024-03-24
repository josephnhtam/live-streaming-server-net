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
        IAsyncEnumerable<(WatchEventType, V1Pod)> WatchPodAsync(CancellationToken cancellationToken = default, TimeSpan? reconnectCheck = null);
    }
}
