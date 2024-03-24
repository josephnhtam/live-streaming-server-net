using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IFleetStateFetcher
    {
        Task<FleetState> GetFleetStateAsync(CancellationToken cancellationToken);
    }
}
