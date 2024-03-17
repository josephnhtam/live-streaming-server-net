using k8s;
using k8s.Models;
using KubeOps.Abstractions.Events;
using KubeOps.KubernetesClient;
using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Models;
using LiveStreamingServerNet.Operator.Services.Contracts;
using LiveStreamingServerNet.Operator.Utilities;
using Polly;

namespace LiveStreamingServerNet.Operator.Services
{
    public class DesiredStateApplier : IDesiredStateApplier
    {
        private readonly IKubernetes _client;
        private readonly IKubernetesClient _operatorClient;
        private readonly EventPublisher _eventPublisher;
        private readonly ILogger _logger;

        private readonly string _podNamespace;
        private readonly ResiliencePipeline _pipeline;

        public DesiredStateApplier(
            IKubernetes client,
            IKubernetesClient operatorClient,
            EventPublisher eventPublisher,
            [FromKeyedServices("k8s-pipeline")] ResiliencePipeline pipeline,
            ILogger<DesiredStateApplier> logger)
        {
            _client = client;
            _operatorClient = operatorClient;
            _eventPublisher = eventPublisher;
            _pipeline = pipeline;
            _logger = logger;
            _podNamespace = _operatorClient.GetCurrentNamespace();
        }

        public async Task ApplyDesiredStateAsync(
            V1LiveStreamingServerCluster entity,
            ClusterState currentState,
            DesiredClusterStateChange desiredStateChange,
            CancellationToken cancellationToken)
        {
            await ApplyPodStateChangesAsync(desiredStateChange.PodStateChanges, cancellationToken);
            await CreateNewPodsAsync(entity, desiredStateChange.PodsCountDelta, cancellationToken);
        }

        private async Task CreateNewPodsAsync(V1LiveStreamingServerCluster entity, int podCountDelta, CancellationToken cancellationToken)
        {
            if (podCountDelta <= 0)
                return;

            var podSpec = entity.Spec.PodSpec;
            podSpec.RestartPolicy = "Never";

            await Task.WhenAll(Enumerable.Range(0, podCountDelta).Select(async _ =>
            {
                var job = new V1Job
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = $"live-streaming-server-net-pod-job-{Guid.NewGuid()}",
                        NamespaceProperty = _podNamespace
                    },
                    Spec = new V1JobSpec
                    {
                        Template = new V1PodTemplateSpec
                        {
                            Metadata = new V1ObjectMeta
                            {
                                Labels = new Dictionary<string, string>
                                {
                                    [Constants.PendingStopLabel] = "false"
                                },
                                Annotations = new Dictionary<string, string>
                                {
                                    [Constants.StreamsCountAnnotation] = "0"
                                }
                            },
                            Spec = podSpec
                        }
                    }
                };

                try
                {
                    await _pipeline.ExecuteAsync(async (cancellationToken) =>
                        await _client.BatchV1.CreateNamespacedJobAsync(job, _podNamespace, cancellationToken: cancellationToken),
                        cancellationToken
                    );
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private async Task ApplyPodStateChangesAsync(IReadOnlyList<PodStateChange> podStateChanges, CancellationToken cancellationToken)
        {
            await Task.WhenAll(podStateChanges.Select(async (podStateChange) =>
            {
                var podPatchBuilder = PodPatcherBuilder.Create();
                podPatchBuilder.SetAnnotation(Constants.PendingStopLabel, podStateChange.PendingStop.ToString());
                var patch = podPatchBuilder.Build();

                try
                {
                    await _pipeline.ExecuteAsync(async (cancellationToken) =>
                        await _client.CoreV1.PatchNamespacedPodAsync(
                            body: patch,
                            name: podStateChange.PodName,
                            namespaceParameter: _podNamespace,
                            cancellationToken: cancellationToken
                        ),
                        cancellationToken
                    );
                }
                catch (Exception ex)
                {
                    //_logger.ErrorPatchingPod(JsonSerializer.Serialize(patch.Content), ex);
                }
            }));
        }
    }
}
