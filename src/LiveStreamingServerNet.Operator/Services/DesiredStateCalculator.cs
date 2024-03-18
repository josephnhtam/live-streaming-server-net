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
            var activePodStates = currentState.PodStates.Where(x => x.Phase <= PodPhase.Running).ToList();
            var podStateChanges = new List<PodStateChange>();

            var desiredPodsCount = CalculateDesiredPodsCount(entity, activePodStates);

            var desiredPodsCountDelta = desiredPodsCount - currentState.PodStates.Where(p => !p.PendingStop).Count();
            if (desiredPodsCountDelta > 0)
            {
                RemovePendingStops(ref desiredPodsCountDelta, entity, podStateChanges, activePodStates);
            }
            else if (desiredPodsCountDelta < 0)
            {
                AddPendingStops(desiredPodsCountDelta, podStateChanges, activePodStates);
            }

            return ValueTask.FromResult(new DesiredClusterStateChange(desiredPodsCountDelta, podStateChanges));
        }

        private void RemovePendingStops(
            ref int desiredPodsCountDelta,
            V1LiveStreamingServerCluster entity,
            List<PodStateChange> podStateChanges,
            IReadOnlyList<PodState> activePodStates)
        {
            var requiredAvailability = desiredPodsCountDelta * entity.Spec.PodStreamsLimit;
            var availabilityRecovered = 0;

            var podsToRemovePendingStop = activePodStates
                .Where(p => p.PendingStop && p.StartTime.HasValue)
                .OrderBy(p => p.StreamsCount)
                .ThenBy(p => p.StartTime)
                .ToList();

            foreach (var pod in podsToRemovePendingStop)
            {
                if (requiredAvailability <= availabilityRecovered)
                    break;

                podStateChanges.Add(new PodStateChange(pod.PodName, false));
                availabilityRecovered += entity.Spec.PodStreamsLimit - pod.StreamsCount;
            }

            desiredPodsCountDelta = Math.Max(0, desiredPodsCountDelta - (availabilityRecovered / entity.Spec.PodStreamsLimit));
        }

        private void AddPendingStops(
            int desiredPodsCountDelta,
            List<PodStateChange> podStateChanges,
            IReadOnlyList<PodState> activePodStates)
        {
            desiredPodsCountDelta = Math.Abs(desiredPodsCountDelta);

            var podsToAddPendingStop = activePodStates
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

        private int CalculateDesiredPodsCount(V1LiveStreamingServerCluster entity, IReadOnlyList<PodState> activePodStates)
        {
            if (activePodStates.Count == 0)
                return entity.Spec.MinReplicas;

            var currentUtilization = (float)activePodStates.Where(p => !p.PendingStop).Sum(p => p.StreamsCount) /
                (activePodStates.Count * entity.Spec.PodStreamsLimit);

            var desiredPodsCount = (int)Math.Ceiling(activePodStates.Count * (currentUtilization / entity.Spec.TargetUtilization));

            return Math.Clamp(desiredPodsCount, entity.Spec.MinReplicas, entity.Spec.MaxReplicas);
        }
    }
}
