using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Models;
using LiveStreamingServerNet.Operator.Services.Contracts;

namespace LiveStreamingServerNet.Operator.Services
{
    public class DesiredStateCalculator : IDesiredStateCalculator
    {
        public ValueTask<DesiredClusterStateChange> CalculateDesiredStateChange(
            V1LiveStreamingServerCluster entity,
            ClusterState currentState,
            CancellationToken cancellationToken)
        {
            var podStateChanges = new List<PodStateChange>();

            var desiredPodsCount = CalculateDesiredPodsCount(entity, currentState);

            var desiredPodsCountDelta = desiredPodsCount - currentState.PodStates.Where(p => !p.PendingStop).Count();
            if (desiredPodsCountDelta > 0)
            {
                RemovePendingStops(ref desiredPodsCountDelta, entity, podStateChanges, currentState);
            }
            else if (desiredPodsCountDelta < 0)
            {
                AddPendingStops(desiredPodsCountDelta, podStateChanges, currentState);
            }

            return ValueTask.FromResult(new DesiredClusterStateChange(desiredPodsCountDelta, podStateChanges));
        }

        private void RemovePendingStops(
            ref int desiredPodsCountDelta,
            V1LiveStreamingServerCluster entity,
            List<PodStateChange> podStateChanges,
            ClusterState currentState)
        {
            var requiredAvailability = desiredPodsCountDelta * entity.Spec.PodSpec.StreamsLimit;
            var availabilityRecovered = 0;

            var podsToRemovePendingStop = currentState.PodStates
                .Where(p => p.PendingStop && p.StartTime.HasValue)
                .OrderBy(p => p.StreamsCount)
                .ThenBy(p => p.StartTime)
                .ToList();

            foreach (var pod in podsToRemovePendingStop)
            {
                if (requiredAvailability <= availabilityRecovered)
                    break;

                podStateChanges.Add(new PodStateChange(pod.PodName, false));
                availabilityRecovered += entity.Spec.PodSpec.StreamsLimit - pod.StreamsCount;
            }

            desiredPodsCountDelta = Math.Max(0, desiredPodsCountDelta - (availabilityRecovered / entity.Spec.PodSpec.StreamsLimit));
        }

        private void AddPendingStops(
            int desiredPodsCountDelta,
            List<PodStateChange> podStateChanges,
            ClusterState currentState)
        {
            desiredPodsCountDelta = Math.Abs(desiredPodsCountDelta);

            var podsToAddPendingStop = currentState.PodStates
                .Where(p => !p.PendingStop && p.StartTime.HasValue)
                .OrderBy(p => p.StreamsCount)
                .ThenBy(p => p.StartTime)
                .ToList();

            for (int i = 0; i < desiredPodsCountDelta && i < podsToAddPendingStop.Count; i++)
            {
                var pod = podsToAddPendingStop[i];
                podStateChanges.Add(new PodStateChange(pod.PodName, true));
            }
        }

        private int CalculateDesiredPodsCount(V1LiveStreamingServerCluster entity, ClusterState currentState)
        {
            var currentUtilization = (float)currentState.PodStates.Where(p => !p.PendingStop).Sum(p => p.StreamsCount) /
                (currentState.PodsCount * entity.Spec.PodSpec.StreamsLimit);

            return (int)Math.Ceiling(currentState.PodsCount * (currentUtilization / entity.Spec.TargetUtilization));
        }
    }
}
