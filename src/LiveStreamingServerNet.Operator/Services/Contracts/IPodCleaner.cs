using LiveStreamingServerNet.Operator.Models;

namespace LiveStreamingServerNet.Operator.Services.Contracts
{
    public interface IPodCleaner
    {
        Task PerformPodCleanupAsync(ClusterState currentState, CancellationToken cancellationToken);
    }
}
