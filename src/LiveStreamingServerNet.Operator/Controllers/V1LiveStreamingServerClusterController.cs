using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Logging;
using LiveStreamingServerNet.Operator.Services.Contracts;

namespace LiveStreamingServerNet.Operator.Controllers
{
    [EntityRbac(typeof(V1LiveStreamingServerCluster), Verbs = RbacVerb.All)]
    [EntityRbac(typeof(V1Pod), Verbs = RbacVerb.All)]
    public class V1LiveStreamingServerClusterController : IEntityController<V1LiveStreamingServerCluster>
    {
        private readonly EntityRequeue<V1LiveStreamingServerCluster> _requeue;
        private readonly IClusterStateRetriver _clusterStateRetriver;
        private readonly IDesiredStateCalculator _desiredStateCalculator;
        private readonly IDesiredStateApplier _desiredStateApplier;
        private readonly IPodCleaner _podCleaner;
        private readonly ILogger _logger;

        public V1LiveStreamingServerClusterController(
            EntityRequeue<V1LiveStreamingServerCluster> requeue,
            IClusterStateRetriver clusterStateRetriver,
            IDesiredStateCalculator desiredStateCalculator,
            IDesiredStateApplier desiredStateApplier,
            IPodCleaner podCleaner,
            ILogger<V1LiveStreamingServerClusterController> logger)
        {
            _requeue = requeue;
            _clusterStateRetriver = clusterStateRetriver;
            _desiredStateCalculator = desiredStateCalculator;
            _desiredStateApplier = desiredStateApplier;
            _podCleaner = podCleaner;
            _logger = logger;
        }

        public async Task ReconcileAsync(V1LiveStreamingServerCluster entity, CancellationToken cancellationToken)
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
                _logger.ReconilingError(ex);
            }
            finally
            {
                _requeue(entity, TimeSpan.FromSeconds(5));
            }
        }

        public Task DeletedAsync(V1LiveStreamingServerCluster entity, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
