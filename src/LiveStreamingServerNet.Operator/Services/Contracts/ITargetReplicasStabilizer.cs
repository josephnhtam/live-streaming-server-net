using LiveStreamingServerNet.Operator.Entities;

namespace LiveStreamingServerNet.Operator.Services.Contracts
{
    public interface ITargetReplicasStabilizer
    {
        int StabilizeTargetReplicas(V1LiveStreamingServerCluster entity, int activePods, int targetReplicas);
    }
}
