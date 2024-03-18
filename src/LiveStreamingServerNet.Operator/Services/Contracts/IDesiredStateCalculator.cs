using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Models;

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
