using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Logging;
using LiveStreamingServerNet.Operator.Services.Contracts;

namespace LiveStreamingServerNet.Operator.Services
{
    public class ClusterScaler : IClusterScaler
    {
        private readonly IClusterStateRetriver _clusterStateRetriver;
        private readonly IDesiredStateCalculator _desiredStateCalculator;
        private readonly IDesiredStateApplier _desiredStateApplier;
        private readonly IPodCleaner _podCleaner;
        private readonly ILogger _logger;

        public ClusterScaler(
            IClusterStateRetriver clusterStateRetriver,
            IDesiredStateCalculator desiredStateCalculator,
            IDesiredStateApplier desiredStateApplier,
            IPodCleaner podCleaner,
            ILogger<ClusterScaler> logger)
        {
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
            }
            catch (Exception ex)
            {
                _logger.ScalingClusterError(ex);
                throw;
            }
        }
    }
}
