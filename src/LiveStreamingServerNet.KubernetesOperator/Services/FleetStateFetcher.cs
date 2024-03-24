using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;
using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Models;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Services
{
    public class FleetStateFetcher : IFleetStateFetcher
    {
        private readonly IKubernetes _client;
        private readonly string _podsNamespace;

        private const int PodChunkSize = 50;

        public FleetStateFetcher(IKubernetes client, IKubernetesClient kubOpsClient)
        {
            _client = client;
            _podsNamespace = kubOpsClient.GetCurrentNamespace();
        }

        public async Task<FleetState> GetFleetStateAsync(V1LiveStreamingServerFleet entity, CancellationToken cancellationToken)
        {
            var pods = new List<PodState>();

            var continuationToken = string.Empty;
            do
            {
                var podList = await _client.CoreV1.ListNamespacedPodAsync(
                    namespaceParameter: _podsNamespace,
                    labelSelector: $"{PodConstants.TypeLabel}={PodConstants.TypeValue}",
                    limit: PodChunkSize,
                    continueParameter: continuationToken,
                    cancellationToken: cancellationToken);

                pods.AddRange(
                    podList.Items
                        .Where(pod => pod.IsOwnedBy(entity))
                        .Select(ResolvePodState)
                );

                continuationToken = podList.Continue();
            } while (!string.IsNullOrEmpty(continuationToken));

            return new FleetState(pods);
        }

        private static PodState ResolvePodState(V1Pod pod)
        {
            var podName = pod.Metadata.Name;
            var startTime = pod.Status.StartTime;
            var pendingStop = bool.TryParse(pod.GetLabel(PodConstants.PendingStopLabel), out var _pendingStop) && _pendingStop;
            var streamCount = int.TryParse(pod.GetAnnotation(PodConstants.StreamsCountAnnotation), out var _streamCount) ? _streamCount : 0;

            var phase = pod.Metadata.DeletionTimestamp.HasValue ?
                PodPhase.Terminating :
                pod.Status.Phase.ToLower() switch
                {
                    "pending" => PodPhase.Pending,
                    "running" => PodPhase.Running,
                    "succeeded" => PodPhase.Succeeded,
                    "failed" => PodPhase.Failed,
                    _ => PodPhase.Unknown
                };

            return new PodState(podName, pendingStop, streamCount, phase, startTime);
        }
    }
}
