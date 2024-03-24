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
        private readonly IFleetStateRetriver _fleetStateRetriver;
        private readonly IDesiredStateCalculator _desiredStateCalculator;
        private readonly IDesiredStateApplier _desiredStateApplier;
        private readonly IPodCleaner _podCleaner;
        private readonly ILogger _logger;

        public FleetScaler(
            IKubernetesClient client,
            IFleetStateRetriver fleetStateRetriver,
            IDesiredStateCalculator desiredStateCalculator,
            IDesiredStateApplier desiredStateApplier,
            IPodCleaner podCleaner,
            ILogger<FleetScaler> logger)
        {
            _client = client;
            _fleetStateRetriver = fleetStateRetriver;
            _desiredStateCalculator = desiredStateCalculator;
            _desiredStateApplier = desiredStateApplier;
            _podCleaner = podCleaner;
            _logger = logger;
        }

        public async Task ScaleFleetAsync(V1LiveStreamingServerFleet entity, CancellationToken cancellationToken)
        {
            try
            {
                var currentState = await _fleetStateRetriver.GetFleetStateAsync(cancellationToken);
                _logger.LogCurrentState(currentState);

                var desiredStateChange = await _desiredStateCalculator.CalculateDesiredStateChange(entity, currentState, cancellationToken);
                _logger.LogDesiredFleetStateChange(desiredStateChange);

                await _desiredStateApplier.ApplyDesiredStateAsync(entity, currentState, desiredStateChange, cancellationToken);

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
