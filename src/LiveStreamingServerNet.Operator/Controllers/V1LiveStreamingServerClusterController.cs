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
        private readonly IClusterScaler _clusterScaler;
        private readonly ILogger _logger;

        public V1LiveStreamingServerClusterController(
            EntityRequeue<V1LiveStreamingServerCluster> requeue,
            IClusterScaler clusterScaler,
            ILogger<V1LiveStreamingServerClusterController> logger)
        {
            _requeue = requeue;
            _clusterScaler = clusterScaler;
            _logger = logger;
        }

        public async Task ReconcileAsync(V1LiveStreamingServerCluster entity, CancellationToken cancellationToken)
        {
            try
            {
                await _clusterScaler.ScaleClusterAsync(entity, cancellationToken);
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
