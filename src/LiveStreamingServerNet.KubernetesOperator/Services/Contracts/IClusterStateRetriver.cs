using LiveStreamingServerNet.KubernetesOperator.Models;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IClusterStateRetriver
    {
        Task<ClusterState> GetClusterStateAsync(CancellationToken cancellationToken);
    }
}
