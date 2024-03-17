using LiveStreamingServerNet.Operator.Models;

namespace LiveStreamingServerNet.Operator.Services.Contracts
{
    public interface IClusterStateRetriver
    {
        Task<ClusterState> GetClusterStateAsync(CancellationToken cancellationToken);
    }
}
