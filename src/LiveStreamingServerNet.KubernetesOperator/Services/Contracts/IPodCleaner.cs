using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IPodCleaner
    {
        Task PerformPodCleanupAsync(ClusterState currentState, CancellationToken cancellationToken);
    }
}
