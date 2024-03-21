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

            var currentUtilization = CalculateUtilization(entity, activePodStates);
            var desiredPodsCount = CalculateDesiredPodsCount(entity, activePodStates, currentUtilization);
            var desiredStateChanges = CalculateStateChanges(entity, currentState, activePodStates, desiredPodsCount);

            if (activePodStates.Count > entity.Spec.MinReplicas)
            {
                var predictedUtilization = PredictUtilization(entity, activePodStates, desiredStateChanges);

                if (!AreStateChangesValid(entity, currentUtilization, predictedUtilization))
                    desiredStateChanges = new DesiredClusterStateChange(0, new List<PodStateChange>());
            }

            return ValueTask.FromResult(desiredStateChanges);
        }

        private static bool AreStateChangesValid(V1LiveStreamingServerCluster entity, float currentUtilization, float predictedUtilization)
        {
            if (currentUtilization < entity.Spec.TargetUtilization)
                return predictedUtilization >= currentUtilization && predictedUtilization <= entity.Spec.TargetUtilization;

            return predictedUtilization < currentUtilization;
        }

        private DesiredClusterStateChange CalculateStateChanges(
            V1LiveStreamingServerCluster entity,
            ClusterState currentState,
            List<PodState> activePodStates,
            int desiredPodsCount)
        {
            var podStateChanges = new List<PodStateChange>();

            var desiredPodsCountDelta =
                            desiredPodsCount - currentState.PodStates.Where(p => !p.PendingStop && p.Phase < PodPhase.Terminating).Count();

            if (desiredPodsCountDelta > 0)
            {
                RemovePendingStops(ref desiredPodsCountDelta, entity, podStateChanges, activePodStates);
            }
            else if (desiredPodsCountDelta < 0)
            {
                AddPendingStops(desiredPodsCountDelta, podStateChanges, activePodStates);
            }

            return new DesiredClusterStateChange((uint)Math.Max(0, desiredPodsCountDelta), podStateChanges);
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
                .Where(p => p.PendingStop && p.StartTime.HasValue && p.Phase < PodPhase.Terminating)
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
                .Where(p => !p.PendingStop && p.StartTime.HasValue && p.Phase < PodPhase.Terminating)
                .OrderBy(p => p.StreamsCount)
                .ThenBy(p => p.StartTime)
                .ToList();

            for (int i = 0; i < desiredPodsCountDelta && i < podsToAddPendingStop.Count; i++)
            {
                var pod = podsToAddPendingStop[i];
                podStateChanges.Add(new PodStateChange(pod.PodName, true));
            }
        }

        private static int CalculateDesiredPodsCount(V1LiveStreamingServerCluster entity, IReadOnlyList<PodState> activePodStates, float currentUtilization)
        {
            if (activePodStates.Count == 0)
                return entity.Spec.MinReplicas;

            var desiredPodsCount = (int)Math.Ceiling(activePodStates.Count * (currentUtilization / entity.Spec.TargetUtilization));
            return Math.Clamp(desiredPodsCount, entity.Spec.MinReplicas, entity.Spec.MaxReplicas);
        }

        private static float CalculateUtilization(V1LiveStreamingServerCluster entity, IReadOnlyList<PodState> activePodStates)
        {
            return (float)activePodStates.Where(p => !p.PendingStop).Sum(p => p.StreamsCount) / (activePodStates.Count * entity.Spec.PodStreamsLimit);
        }

        private static float PredictUtilization(V1LiveStreamingServerCluster entity,
            IReadOnlyList<PodState> activePodStates, DesiredClusterStateChange stateChanges)
        {
            var predictedActivePodStates = new List<PodState>(activePodStates);
            var podStateChanges = stateChanges.PodStateChanges.ToDictionary(x => x.PodName);

            for (int i = 0; i < predictedActivePodStates.Count; i++)
            {
                PodState podState = predictedActivePodStates[i];

                if (podStateChanges.TryGetValue(podState.PodName, out var change))
                {
                    podState = podState with { PendingStop = change.PendingStop };
                    predictedActivePodStates[i] = podState;
                }
            }

            for (int i = 0; i < stateChanges.PodsIncrement; i++)
                predictedActivePodStates.Add(new PodState(Guid.NewGuid().ToString(), false, 0, PodPhase.Running, DateTime.UtcNow));

            return CalculateUtilization(entity, predictedActivePodStates);
        }
    }
}
