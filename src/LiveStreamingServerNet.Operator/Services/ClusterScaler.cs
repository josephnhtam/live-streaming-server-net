using KubeOps.KubernetesClient;
using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Logging;
using LiveStreamingServerNet.Operator.Models;
using LiveStreamingServerNet.Operator.Services.Contracts;

namespace LiveStreamingServerNet.Operator.Services
{
    public class ClusterScaler : IClusterScaler
    {
        private readonly IKubernetesClient _client;
        private readonly IClusterStateRetriver _clusterStateRetriver;
        private readonly IDesiredStateCalculator _desiredStateCalculator;
        private readonly IDesiredStateApplier _desiredStateApplier;
        private readonly IPodCleaner _podCleaner;
        private readonly ILogger _logger;

        public ClusterScaler(
            IKubernetesClient client,
            IClusterStateRetriver clusterStateRetriver,
            IDesiredStateCalculator desiredStateCalculator,
            IDesiredStateApplier desiredStateApplier,
            IPodCleaner podCleaner,
            ILogger<ClusterScaler> logger)
        {
            _client = client;
            _clusterStateRetriver = clusterStateRetriver;
            _desiredStateCalculator = desiredStateCalculator;
            _desiredStateApplier = desiredStateApplier;
            _podCleaner = podCleaner;
            _logger = logger;
        }

        public async Task ScaleClusterAsync(V1LiveStreamingServerCluster entity, CancellationToken cancellationToken)
        {
            try
            {
                var currentState = await _clusterStateRetriver.GetClusterStateAsync(cancellationToken);
                _logger.LogCurrentState(currentState);

                var desiredStateChange = await _desiredStateCalculator.CalculateDesiredStateChange(entity, currentState, cancellationToken);
                _logger.LogDesiredClusterStateChange(desiredStateChange);

                await _desiredStateApplier.ApplyDesiredStateAsync(entity, currentState, desiredStateChange, cancellationToken);

                await _podCleaner.PerformPodCleanupAsync(currentState, cancellationToken);

                await UpdateStatusAsync(entity, currentState, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.ScalingClusterError(ex);
                throw;
            }
        }

        private async Task UpdateStatusAsync(V1LiveStreamingServerCluster entity, ClusterState currentState, CancellationToken cancellationToken)
        {
            entity.Status = new V1LiveStreamingServerCluster.EntityStatus
            {
                ActivePods = currentState.ActivePods.Count,
                PendingStopPods = currentState.PendingStopPods.Count,
                TotalStreams = currentState.TotalStreams,
            };

            await _client.UpdateStatusAsync(entity, cancellationToken);
        }
    }
}
