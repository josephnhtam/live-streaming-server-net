using k8s;
using k8s.Models;
using KubeOps.Abstractions.Events;
using KubeOps.KubernetesClient;
using LiveStreamingServerNet.Operator.Entities;
using LiveStreamingServerNet.Operator.Logging;
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
        private readonly ResiliencePipeline _pipeline;
        private readonly ILogger _logger;

        private readonly string _podNamespace;

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
            await CreateNewPodsAsync(entity, desiredStateChange.PodsIncrement, cancellationToken);
        }

        private async Task CreateNewPodsAsync(V1LiveStreamingServerCluster entity, uint podsIncrement, CancellationToken cancellationToken)
        {
            if (podsIncrement == 0)
                return;

            var template = CreatePodTemplate(entity);

            await Task.WhenAll(Enumerable.Range(0, (int)podsIncrement).Select(async _ =>
            {
                var pod = new V1Pod
                {
                    Metadata = new V1ObjectMeta
                    {
                        GenerateName = !string.IsNullOrEmpty(template.Metadata.GenerateName) ?
                            template.Metadata.GenerateName :
                            $"{template.Metadata.Name}-",

                        NamespaceProperty = _podNamespace,
                        Labels = template.Metadata.Labels,
                        Annotations = template.Metadata.Annotations,

                        OwnerReferences = new List<V1OwnerReference>
                        {
                            new V1OwnerReference(
                                apiVersion: entity.ApiVersion,
                                kind: entity.Kind,
                                name: entity.Name(),
                                uid: entity.Uid()
                            )
                        }
                    },
                    Spec = template.Spec,
                };

                try
                {
                    await _pipeline.ExecuteAsync(async (cancellationToken) =>
                        await _client.CoreV1.CreateNamespacedPodAsync(pod, _podNamespace, cancellationToken: cancellationToken),
                        cancellationToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.CreatingPodError(ex);
                }
            }));
        }

        private static V1PodTemplateSpec CreatePodTemplate(V1LiveStreamingServerCluster entity)
        {
            var template = entity.Spec.Template;

            template.Metadata.Labels ??= new Dictionary<string, string>();
            template.Metadata.Labels[Constants.AppLabel] = Constants.AppLabelValue;
            template.Metadata.Labels[Constants.PendingStopLabel] = "false";

            template.Metadata.Annotations ??= new Dictionary<string, string>();
            template.Metadata.Annotations[Constants.StreamsCountAnnotation] = "0";

            template.Spec.RestartPolicy = "Never";

            return template;
        }

        private async Task ApplyPodStateChangesAsync(IReadOnlyList<PodStateChange> podStateChanges, CancellationToken cancellationToken)
        {
            await Task.WhenAll(podStateChanges.Select(async (podStateChange) =>
            {
                var podPatchBuilder = PodPatcherBuilder.Create();
                podPatchBuilder.SetLabel(Constants.PendingStopLabel, podStateChange.PendingStop.ToString().ToLower());
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
                    _logger.PatchingPodError(podStateChange.PodName, ex);
                }
            }));
        }
    }
}
