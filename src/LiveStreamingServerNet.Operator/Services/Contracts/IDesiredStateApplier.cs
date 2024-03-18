using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Models;

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
