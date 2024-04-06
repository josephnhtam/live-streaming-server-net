using LiveStreamingServerNet.KubernetesOperator.Entities;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IFleetScalerResolver
    {
        IFleetScaler Resolve(V1LiveStreamingServerFleet entity);
        void Finalize(V1LiveStreamingServerFleet entity);
    }
}
