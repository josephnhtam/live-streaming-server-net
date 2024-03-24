using k8s;
using KubeOps.KubernetesClient;
using LiveStreamingServerNet.KubernetesOperator.Logging;
using LiveStreamingServerNet.KubernetesOperator.Models;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;
using Polly;

namespace LiveStreamingServerNet.KubernetesOperator.Services
{
    public class PodCleaner : IPodCleaner
    {
        private readonly IKubernetes _client;
        private readonly ResiliencePipeline _pipeline;
        private readonly ILogger _logger;

        private readonly string _podNamespace;

        public PodCleaner(
            IKubernetes client,
            IKubernetesClient operatorClient,
            [FromKeyedServices("k8s-pipeline")] ResiliencePipeline pipeline,
            ILogger<PodCleaner> logger)
        {
            _client = client;
            _pipeline = pipeline;
            _logger = logger;
            _podNamespace = operatorClient.GetCurrentNamespace();
        }

        public async Task PerformPodCleanupAsync(FleetState currentState, CancellationToken cancellationToken)
        {
            var completePods = currentState.Pods.Where(p => p.Phase >= PodPhase.Succeeded);

            await Task.WhenAll(completePods.Select(async pod =>
                {
                    try
                    {
                        await _pipeline.ExecuteAsync(async _ =>
                            await _client.CoreV1.DeleteNamespacedPodAsync(
                                name: pod.PodName,
                                namespaceParameter: _podNamespace,
                                cancellationToken: cancellationToken)
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.DeletingPodError(pod.PodName, ex);
                    }
                }
            ));
        }
    }
}
