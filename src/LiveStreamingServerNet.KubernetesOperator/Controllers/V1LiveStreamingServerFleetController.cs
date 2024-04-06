using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Logging;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Controllers
{
    [EntityRbac(typeof(V1LiveStreamingServerFleet), Verbs = RbacVerb.All)]
    [EntityRbac(typeof(V1Pod), Verbs = RbacVerb.All)]
    public class V1LiveStreamingServerFleetController : IEntityController<V1LiveStreamingServerFleet>
    {
        private readonly EntityRequeue<V1LiveStreamingServerFleet> _requeue;
        private readonly IFleetScalerResolver _fleetScalerResolver;
        private readonly ILogger _logger;

        public V1LiveStreamingServerFleetController(
            EntityRequeue<V1LiveStreamingServerFleet> requeue,
            IFleetScalerResolver fleetScalerResolver,
            ILogger<V1LiveStreamingServerFleetController> logger)
        {
            _requeue = requeue;
            _fleetScalerResolver = fleetScalerResolver;
            _logger = logger;
        }

        public async Task ReconcileAsync(V1LiveStreamingServerFleet entity, CancellationToken cancellationToken)
        {
            try
            {
                var feleetScaler = _fleetScalerResolver.Resolve(entity);
                await feleetScaler.ScaleFleetAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.ReconilingError(ex);
            }
            finally
            {
                _requeue(entity, TimeSpan.FromSeconds(entity.Spec.SyncPeriodSeconds));
            }
        }

        public Task DeletedAsync(V1LiveStreamingServerFleet entity, CancellationToken cancellationToken)
        {
            _fleetScalerResolver.Finalize(entity);
            return Task.CompletedTask;
        }
    }
}
