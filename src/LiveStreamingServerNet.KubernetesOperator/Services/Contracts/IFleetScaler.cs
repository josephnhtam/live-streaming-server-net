using LiveStreamingServerNet.KubernetesOperator.Entities;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IFleetScaler
    {
        Task ScaleFleetAsync(V1LiveStreamingServerFleet entity, CancellationToken cancellationToken);
    }
}
