using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IDesiredStateCalculator
    {
        ValueTask<DesiredClusterStateChange> CalculateDesiredStateChange(
            V1LiveStreamingServerCluster entity,
            ClusterState currentState,
            CancellationToken cancellationToken);
    }
}
