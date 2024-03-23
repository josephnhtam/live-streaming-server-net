using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;
using LiveStreamingServerNet.Operator.Models;
using LiveStreamingServerNet.Operator.Services.Contracts;

namespace LiveStreamingServerNet.Operator.Services
{
    public class ClusterStateRetriver : IClusterStateRetriver
    {
        private readonly IKubernetes _client;
        private readonly string _podsNamespace;

        public ClusterStateRetriver(IKubernetes client, IKubernetesClient kubOpsClient)
        {
            _client = client;
            _podsNamespace = kubOpsClient.GetCurrentNamespace();
        }

        public async Task<ClusterState> GetClusterStateAsync(CancellationToken cancellationToken)
        {
            var pods = new List<PodState>();

            bool hasNext = false;
            do
            {
                var podList = await _client.CoreV1.ListNamespacedPodAsync(
                    namespaceParameter: _podsNamespace,
                    labelSelector: $"{PodConstants.TypeLabel}={PodConstants.TypeValue}",
                    cancellationToken: cancellationToken);

                pods.AddRange(podList.Items.Select(ResolvePodState));

                hasNext = !string.IsNullOrEmpty(podList.Continue());
            } while (hasNext);

            return new ClusterState(pods);
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
