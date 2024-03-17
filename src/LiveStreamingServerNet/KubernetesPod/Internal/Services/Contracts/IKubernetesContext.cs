using k8s;
using k8s.Models;

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

    internal interface IPodPatcherBuilder
    {
        IPodPatcherBuilder SetLabel(string key, string value);
        IPodPatcherBuilder RemoveLabel(string key);
        IPodPatcherBuilder SetAnnotation(string key, string value);
        IPodPatcherBuilder RemoveAnnotation(string key);
    }
}
