using k8s.Models;
using KubeOps.Abstractions.Controller;
using KubeOps.Abstractions.Queue;
using KubeOps.Abstractions.Rbac;
using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Services.Contracts;
using System.Text.Json;

namespace LiveStreamingServerNet.Operator.Controllers
{
    [EntityRbac(typeof(V1LiveStreamingServerCluster), Verbs = RbacVerb.All)]
    [EntityRbac(typeof(V1Job), Verbs = RbacVerb.Create)]
    [EntityRbac(typeof(V1Pod), Verbs = RbacVerb.Get | RbacVerb.List | RbacVerb.Patch)]
    public class V1LiveStreamingServerClusterController : IEntityController<V1LiveStreamingServerCluster>
    {
        private readonly EntityRequeue<V1LiveStreamingServerCluster> _requeue;
        private readonly IClusterStateRetriver _clusterStateRetriver;
        private readonly IDesiredStateCalculator _desiredStateCalculator;
        private readonly IDesiredStateApplier _desiredStateApplier;

        public V1LiveStreamingServerClusterController(
            EntityRequeue<V1LiveStreamingServerCluster> requeue,
            IClusterStateRetriver clusterStateRetriver,
            IDesiredStateCalculator desiredStateCalculator,
            IDesiredStateApplier desiredStateApplier)
        {
            _requeue = requeue;
            _clusterStateRetriver = clusterStateRetriver;
            _desiredStateCalculator = desiredStateCalculator;
            _desiredStateApplier = desiredStateApplier;
        }

        public async Task ReconcileAsync(V1LiveStreamingServerCluster entity, CancellationToken cancellationToken)
        {
            Console.WriteLine("###ReconcileAsync###");

            try
            {
                var currentState = await _clusterStateRetriver.GetClusterStateAsync(cancellationToken);

                Console.WriteLine("===CurrentState===");
                Console.WriteLine(JsonSerializer.Serialize(currentState));

                var desiredStateChange = await _desiredStateCalculator.CalculateDesiredStateChange(entity, currentState, cancellationToken);

                Console.WriteLine("===DesiredStateChange===");
                Console.WriteLine(JsonSerializer.Serialize(currentState));

                await _desiredStateApplier.ApplyDesiredStateAsync(entity, currentState, desiredStateChange, cancellationToken);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _requeue(entity, TimeSpan.FromSeconds(5));
            }

            Console.WriteLine("#####################");
        }

        public Task DeletedAsync(V1LiveStreamingServerCluster entity, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
