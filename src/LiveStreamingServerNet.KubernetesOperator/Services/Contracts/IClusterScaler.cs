using LiveStreamingServerNet.KubernetesOperator.Entities;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IClusterScaler
    {
        Task ScaleClusterAsync(V1LiveStreamingServerCluster entity, CancellationToken cancellationToken);
    }
}
