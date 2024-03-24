using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IPodCleaner
    {
        Task PerformPodCleanupAsync(FleetState currentState, CancellationToken cancellationToken);
    }
}
