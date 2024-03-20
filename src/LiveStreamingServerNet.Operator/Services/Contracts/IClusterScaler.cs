using LiveStreamingServerNet.Operator.Entities;

namespace LiveStreamingServerNet.Operator.Services.Contracts
{
    public interface IClusterScaler
    {
        Task ScaleClusterAsync(V1LiveStreamingServerCluster entity, CancellationToken cancellationToken);
    }
}
