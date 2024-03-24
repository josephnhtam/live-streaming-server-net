using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Services
{
    public class TargetReplicasStabilizer : ITargetReplicasStabilizer
    {
        private record Recommendation(DateTime Time, int Replicas);

        private List<Recommendation> _recommendations = new();

        public int StabilizeTargetReplicas(V1LiveStreamingServerFleet entity, int activePods, int targetReplicas)
        {
            targetReplicas = Math.Clamp(targetReplicas, entity.Spec.MinReplicas, entity.Spec.MaxReplicas);

            var now = DateTime.Now;

            var upReplicas = targetReplicas;
            var upCutoff = now.AddSeconds(-entity.Spec.ScaleUpStabilizationWindowSeconds);

            var downReplicas = targetReplicas;
            var downCutoff = now.AddSeconds(-entity.Spec.ScaleDownStabilizationWindowSeconds);

            lock (_recommendations)
            {
                for (int i = 0; i < _recommendations.Count; i++)
                {
                    var recommendation = _recommendations[i];

                    if (recommendation.Time > upCutoff)
                    {
                        upReplicas = Math.Min(upReplicas, recommendation.Replicas);
                    }

                    if (recommendation.Time > downCutoff)
                    {
                        downReplicas = Math.Max(downReplicas, recommendation.Replicas);
                    }

                    if (recommendation.Time < upCutoff && recommendation.Time < downCutoff)
                    {
                        _recommendations.RemoveAt(i--);
                    }
                }

                _recommendations.Add(new Recommendation(now, targetReplicas));
            }

            var stabilizedReplicas = activePods;

            if (stabilizedReplicas < upReplicas)
                stabilizedReplicas = upReplicas;

            if (stabilizedReplicas > downReplicas)
                stabilizedReplicas = downReplicas;

            return stabilizedReplicas;
        }
    }
}
