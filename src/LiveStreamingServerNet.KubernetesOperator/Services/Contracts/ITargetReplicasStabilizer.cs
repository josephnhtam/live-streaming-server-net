using LiveStreamingServerNet.KubernetesOperator.Entities;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface ITargetReplicasStabilizer
    {
        int StabilizeTargetReplicas(V1LiveStreamingServerFleet entity, int activePods, int targetReplicas);
    }
}
