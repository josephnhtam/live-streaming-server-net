using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Models;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Services
{
    public class DesiredFleetStateCalculator : IDesiredFleetStateCalculator
    {
        private readonly ITargetReplicasStabilizer _targetReplicasStabilizer;

        public DesiredFleetStateCalculator(ITargetReplicasStabilizer targetReplicasStabilizer)
        {
            _targetReplicasStabilizer = targetReplicasStabilizer;
        }

        public ValueTask<DesiredFleetStateChange> CalculateDesiredStateChange(
            V1LiveStreamingServerFleet entity,
            FleetState currentState,
            CancellationToken cancellationToken)
        {
            var currentUtilization = CalculateUtilization(entity, currentState.ActivePods);
            var desiredPodsCount = CalculateDesiredPodsCount(entity, currentState, currentUtilization);
            var desiredStateChanges = CalculateStateChanges(entity, currentState, desiredPodsCount);

            if (currentState.ActivePods.Count >= entity.Spec.MinReplicas)
            {
                var predictedUtilization = PredictUtilization(entity, currentState, desiredStateChanges);

                if (!AreStateChangesValid(entity, currentUtilization, predictedUtilization))
                    desiredStateChanges = new DesiredFleetStateChange(0, new List<PodStateChange>());
            }

            return ValueTask.FromResult(desiredStateChanges);
        }

        private static bool AreStateChangesValid(V1LiveStreamingServerFleet entity, float currentUtilization, float predictedUtilization)
        {
            if (currentUtilization < entity.Spec.TargetUtilization)
                return predictedUtilization >= currentUtilization && predictedUtilization <= entity.Spec.TargetUtilization;

            return predictedUtilization < currentUtilization;
        }

        private DesiredFleetStateChange CalculateStateChanges(
            V1LiveStreamingServerFleet entity,
            FleetState currentState,
            int desiredPodsCount)
        {
            var podStateChanges = new List<PodStateChange>();

            var desiredPodsCountDelta = desiredPodsCount - currentState.ActivePods.Count;

            if (desiredPodsCountDelta > 0)
            {
                RemovePendingStops(ref desiredPodsCountDelta, entity, podStateChanges, currentState);
            }
            else if (desiredPodsCountDelta < 0)
            {
                AddPendingStops(desiredPodsCountDelta, podStateChanges, currentState);
            }

            return new DesiredFleetStateChange((uint)Math.Max(0, desiredPodsCountDelta), podStateChanges);
        }

        private void RemovePendingStops(
            ref int desiredPodsCountDelta,
            V1LiveStreamingServerFleet entity,
            List<PodStateChange> podStateChanges,
            FleetState currentState)
        {
            var requiredAvailability = desiredPodsCountDelta * entity.Spec.PodStreamsLimit;
            var availabilityRecovered = 0;

            var podsToRemovePendingStop = currentState.Pods
                .Where(p => p.PendingStop && p.StartTime.HasValue && p.Phase <= PodPhase.Running)
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
            FleetState currentState)
        {
            desiredPodsCountDelta = Math.Abs(desiredPodsCountDelta);

            var podsToMarkPendingStop = currentState.ActivePods
                .Where(p => p.StartTime.HasValue)
                .OrderBy(p => p.StreamsCount)
                .ThenBy(p => p.StartTime)
                .ToList();

            for (int i = 0; i < desiredPodsCountDelta && i < podsToMarkPendingStop.Count; i++)
            {
                var pod = podsToMarkPendingStop[i];
                podStateChanges.Add(new PodStateChange(pod.PodName, true));
            }
        }

        private int CalculateDesiredPodsCount(V1LiveStreamingServerFleet entity, FleetState currentState, float currentUtilization)
        {
            var desiredPodsCount = (int)Math.Ceiling(currentState.ActivePods.Count * (currentUtilization / entity.Spec.TargetUtilization));

            desiredPodsCount = _targetReplicasStabilizer.StabilizeTargetReplicas(entity, currentState.ActivePods.Count, desiredPodsCount);

            return Math.Clamp(desiredPodsCount, entity.Spec.MinReplicas, entity.Spec.MaxReplicas);
        }

        private static float CalculateUtilization(V1LiveStreamingServerFleet entity, IReadOnlyList<PodState> activePods)
        {
            return (float)activePods.Sum(p => p.StreamsCount) / (activePods.Count * entity.Spec.PodStreamsLimit);
        }

        private static float PredictUtilization(V1LiveStreamingServerFleet entity, FleetState currentState, DesiredFleetStateChange stateChanges)
        {
            var predictedPods = new List<PodState>(currentState.Pods);
            var podStateChanges = stateChanges.PodStateChanges.ToDictionary(x => x.PodName);

            for (int i = 0; i < predictedPods.Count; i++)
            {
                PodState podState = predictedPods[i];

                if (podStateChanges.TryGetValue(podState.PodName, out var change))
                {
                    podState = podState with { PendingStop = change.PendingStop };
                    predictedPods[i] = podState;
                }
            }

            for (int i = 0; i < stateChanges.PodsIncrement; i++)
                predictedPods.Add(new PodState(Guid.NewGuid().ToString(), false, 0, PodPhase.Running, DateTime.UtcNow));

            var predictedActivePods = predictedPods.Where(x => x.Phase <= PodPhase.Running && !x.PendingStop).ToList();
            return CalculateUtilization(entity, predictedActivePods);
        }
    }
}
