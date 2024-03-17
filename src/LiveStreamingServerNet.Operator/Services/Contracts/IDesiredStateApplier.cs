using LiveStreamingServerNet.Operator.Models;
using LiveStreamingServerNet.Operator.Entities;

namespace LiveStreamingServerNet.Operator.Services.Contracts
{
    public interface IDesiredStateApplier
    {
        Task ApplyDesiredStateAsync(
            V1LiveStreamingServerCluster entity,
            ClusterState currentState,
            DesiredClusterStateChange desiredStateChange,
            CancellationToken cancellationToken);
    }
}
