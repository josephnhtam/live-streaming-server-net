using k8s;
using KubeOps.Abstractions.Events;
using KubeOps.KubernetesClient;
using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Models;
using LiveStreamingServerNet.Operator.Services.Contracts;

namespace LiveStreamingServerNet.Operator.Services
{
    public class DesiredStateApplier : IDesiredStateApplier
    {
        private readonly IKubernetes _client;
        private readonly IKubernetesClient _operatorClient;
        private readonly EventPublisher _eventPublisher;

        public DesiredStateApplier(IKubernetes client, IKubernetesClient operatorClient, EventPublisher eventPublisher)
        {
            _client = client;
            _operatorClient = operatorClient;
            _eventPublisher = eventPublisher;
        }

        public async Task ApplyDesiredStateAsync(
            V1LiveStreamingServerCluster entity,
            ClusterState currentState,
            DesiredClusterStateChange desiredStateChange,
            CancellationToken cancellationToken)
        {
            await ApplyPodStateChanges(desiredStateChange.PodStateChanges);

            throw new NotImplementedException();
        }

        private Task ApplyPodStateChanges(IReadOnlyList<PodStateChange> podStateChanges)
        {
            throw new NotImplementedException();
        }
    }
}
