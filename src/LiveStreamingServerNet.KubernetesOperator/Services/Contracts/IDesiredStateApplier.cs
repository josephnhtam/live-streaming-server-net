using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
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
