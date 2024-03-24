using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IDesiredStateCalculator
    {
        ValueTask<DesiredFleetStateChange> CalculateDesiredStateChange(
            V1LiveStreamingServerFleet entity,
            FleetState currentState,
            CancellationToken cancellationToken);
    }
}
