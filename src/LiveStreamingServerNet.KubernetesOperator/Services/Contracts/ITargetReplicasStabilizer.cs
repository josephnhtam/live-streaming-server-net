using LiveStreamingServerNet.KubernetesOperator.Entities;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface ITargetReplicasStabilizer
    {
        int StabilizeTargetReplicas(V1LiveStreamingServerCluster entity, int activePods, int targetReplicas);
    }
}
