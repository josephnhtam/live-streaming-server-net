using k8s;
using k8s.Models;
using KubeOps.Abstractions.Events;
using KubeOps.KubernetesClient;
using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Logging;
using LiveStreamingServerNet.KubernetesOperator.Models;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;
using LiveStreamingServerNet.KubernetesOperator.Utilities;
using Polly;

namespace LiveStreamingServerNet.KubernetesOperator.Services
{
    public class DesiredFleetStateApplier : IDesiredFleetStateApplier
    {
        private readonly IKubernetes _client;
        private readonly IKubernetesClient _operatorClient;
        private readonly IPodTemplateCreator _podTemplateCreator;
        private readonly EventPublisher _eventPublisher;
        private readonly ResiliencePipeline _pipeline;
        private readonly ILogger _logger;

        private readonly string _podNamespace;

        public DesiredFleetStateApplier(
            IKubernetes client,
            IKubernetesClient operatorClient,
            IPodTemplateCreator podTemplateCreator,
            EventPublisher eventPublisher,
            [FromKeyedServices("k8s-pipeline")] ResiliencePipeline pipeline,
            ILogger<DesiredFleetStateApplier> logger)
        {
            _client = client;
            _operatorClient = operatorClient;
            _podTemplateCreator = podTemplateCreator;
            _eventPublisher = eventPublisher;
            _pipeline = pipeline;
            _logger = logger;
            _podNamespace = _operatorClient.GetCurrentNamespace();
        }

        public async Task ApplyDesiredStateAsync(
            V1LiveStreamingServerFleet entity,
            FleetState currentState,
            DesiredFleetStateChange desiredStateChange,
            CancellationToken cancellationToken)
        {
            await ApplyPodStateChangesAsync(desiredStateChange.PodStateChanges, cancellationToken);
            await CreateNewPodsAsync(entity, desiredStateChange.PodsIncrement, cancellationToken);
        }

        private async Task CreateNewPodsAsync(V1LiveStreamingServerFleet entity, uint podsIncrement, CancellationToken cancellationToken)
        {
            if (podsIncrement == 0)
                return;

            var template = _podTemplateCreator.CreatePodTemplate(entity);

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

        private async Task ApplyPodStateChangesAsync(IReadOnlyList<PodStateChange> podStateChanges, CancellationToken cancellationToken)
        {
            await Task.WhenAll(podStateChanges.Select(async (podStateChange) =>
            {
                var podPatchBuilder = PodPatcherBuilder.Create();
                podPatchBuilder.SetLabel(PodConstants.PendingStopLabel, podStateChange.PendingStop.ToString().ToLower());
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
