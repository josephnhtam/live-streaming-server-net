using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IFleetStateFetcher
    {
        Task<FleetState> GetFleetStateAsync(V1LiveStreamingServerFleet entity, CancellationToken cancellationToken);
    }
}
