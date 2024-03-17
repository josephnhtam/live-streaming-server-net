using LiveStreamingServerNet.Operator.Models;
using LiveStreamingServerNet.Operator.Entities;

namespace LiveStreamingServerNet.Operator.Services.Contracts
{
    public interface IDesiredStateCalculator
    {
        ValueTask<DesiredClusterStateChange> CalculateDesiredStateChange(
            V1LiveStreamingServerCluster entity,
            ClusterState currentState,
            CancellationToken cancellationToken);
    }
}
