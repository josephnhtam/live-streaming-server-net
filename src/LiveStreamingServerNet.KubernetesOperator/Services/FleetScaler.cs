using KubeOps.KubernetesClient;
using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Logging;
using LiveStreamingServerNet.KubernetesOperator.Models;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Services
{
    public class FleetScaler : IFleetScaler
    {
        private readonly IKubernetesClient _client;
        private readonly IFleetStateFetcher _fleetStateFetcher;
        private readonly IDesiredFleetStateCalculator _desiredFleetStateCalculator;
        private readonly IDesiredFleetStateApplier _desiredFleetStateApplier;
        private readonly IPodCleaner _podCleaner;
        private readonly ILogger _logger;

        public FleetScaler(
            IKubernetesClient client,
            IFleetStateFetcher fleetStateFetcher,
            IDesiredFleetStateCalculator desiredFleetStateCalculator,
            IDesiredFleetStateApplier desiredFleetStateApplier,
            IPodCleaner podCleaner,
            ILogger<FleetScaler> logger)
        {
            _client = client;
            _fleetStateFetcher = fleetStateFetcher;
            _desiredFleetStateCalculator = desiredFleetStateCalculator;
            _desiredFleetStateApplier = desiredFleetStateApplier;
            _podCleaner = podCleaner;
            _logger = logger;
        }

        public async Task ScaleFleetAsync(V1LiveStreamingServerFleet entity, CancellationToken cancellationToken)
        {
            try
            {
                var currentState = await _fleetStateFetcher.GetFleetStateAsync(entity, cancellationToken);
                _logger.LogCurrentState(currentState);

                var desiredStateChange = await _desiredFleetStateCalculator.CalculateDesiredStateChange(entity, currentState, cancellationToken);
                _logger.LogDesiredFleetStateChange(desiredStateChange);

                await _desiredFleetStateApplier.ApplyDesiredStateAsync(entity, currentState, desiredStateChange, cancellationToken);

                await _podCleaner.PerformPodCleanupAsync(currentState, cancellationToken);

                await UpdateStatusAsync(entity, currentState, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.ScalingFleetError(ex);
                throw;
            }
        }

        private async Task UpdateStatusAsync(V1LiveStreamingServerFleet entity, FleetState currentState, CancellationToken cancellationToken)
        {
            entity.Status = new V1LiveStreamingServerFleet.EntityStatus
            {
                ActivePods = currentState.ActivePods.Count,
                PendingStopPods = currentState.PendingStopPods.Count,
                TotalStreams = currentState.TotalStreams,
            };

            await _client.UpdateStatusAsync(entity, cancellationToken);
        }
    }
}
