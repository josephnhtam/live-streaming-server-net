using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Models;
using LiveStreamingServerNet.Operator.Services.Contracts;

namespace LiveStreamingServerNet.Operator.Services
{
    // ref: https://github.com/kubernetes/kubernetes/blob/656cb1028ea5af837e69b5c9c614b008d747ab63/pkg/controller/podautoscaler/horizontal.go
    public class DesiredStateStabilizer : IDesiredStateStabilizer
    {
        private List<Recommendation> _recommendations = new();

        public DesiredClusterStateChange StabilizeDesiredStateChange(
            V1LiveStreamingServerCluster entity,
            ClusterState currentState,
            DesiredClusterStateChange desiredStateChange)
        {
            var activePods = currentState.Pods.Where(x => x.Phase <= PodPhase.Running && !x.PendingStop).ToList();

            if (activePods.Count < entity.Spec.MinReplicas || activePods.Count > entity.Spec.MaxReplicas)
                return desiredStateChange;

            var targetReplicas = CaculateTargetReplicas(activePods, desiredStateChange);

            var upReplicas = targetReplicas;
            var upCutoff = DateTime.UtcNow.AddSeconds(-entity.Spec.SccaleUpStabilizationWindowSeconds);

            var downReplicas = targetReplicas;
            var downCutoff = DateTime.UtcNow.AddSeconds(-entity.Spec.SccaleDownStabilizationWindowSeconds);

            lock (_recommendations)
            {
                for (int i = 0; i < _recommendations.Count; i++)
                {
                    var recommendation = _recommendations[i];

                    if (recommendation.Time > upCutoff)
                    {
                        upReplicas = Math.Min(upReplicas, recommendation.Replicas);
                    }
                    else if (recommendation.Time > downCutoff)
                    {
                        downReplicas = Math.Max(downReplicas, recommendation.Replicas);
                    }
                    else
                    {
                        _recommendations.RemoveAt(i--);
                    }
                }

                //var recommendation = 
            }

            throw new NotImplementedException();
        }

        private static int CaculateTargetReplicas(IReadOnlyList<PodState> activePods, DesiredClusterStateChange desiredStateChange)
        {
            return activePods.Count
                + (int)desiredStateChange.PodsIncrement
                + desiredStateChange.PodStateChanges.Count(x => !x.PendingStop)
                - desiredStateChange.PodStateChanges.Count(x => x.PendingStop);
        }

        private record Recommendation(DateTime Time, int Replicas);
    }
}
