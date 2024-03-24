using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IFleetStateRetriver
    {
        Task<FleetState> GetFleetStateAsync(CancellationToken cancellationToken);
    }
}
