using k8s;
using k8s.Models;
using LiveStreamingServerNet.KubernetesPod.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.KubernetesPod.Utilities;
using LiveStreamingServerNet.KubernetesPod.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services
{
    internal class KubernetesContext : IKubernetesContext
    {
        public IKubernetes KubernetesClient { get; }
        public string PodNamespace { get; }
        public string PodName { get; }

        private readonly ILogger _logger;

        public KubernetesContext(ILogger<KubernetesContext> logger)
        {
            _logger = logger;

            KubernetesClient = CreateKubernetesClient();
            PodNamespace = GetPodNamespace();
            PodName = GetPodName();
        }

        private IKubernetes CreateKubernetesClient()
        {
            var kubeConfig = KubernetesClientConfiguration.InClusterConfig();
            return new Kubernetes(kubeConfig);
        }

        private string GetPodNamespace()
        {
            return Environment.GetEnvironmentVariable(PodConstants.PodNamespaceEnv) ??
                throw new InvalidOperationException($"Environment variable '{PodConstants.PodNamespaceEnv}' is not set.");
        }

        private string GetPodName()
        {
            return Environment.GetEnvironmentVariable(PodConstants.PodNameEnv) ??
                throw new InvalidOperationException($"Environment variable '{PodConstants.PodNameEnv}' is not set.");
        }

        public async Task<V1Pod> GetPodAsync(CancellationToken cancellationToken)
        {
            return (await KubernetesClient.CoreV1.ListNamespacedPodAsync(
                namespaceParameter: PodNamespace,
                fieldSelector: $"metadata.name={PodName}",
                cancellationToken: cancellationToken
            )).Items.FirstOrDefault() ??
                throw new InvalidOperationException($"Failed to get the pod '{PodName} in {PodNamespace}");
        }

        public async IAsyncEnumerable<(WatchEventType, V1Pod)> WatchPodAsync(
            [EnumeratorCancellation] CancellationToken stoppingToken = default,
            TimeSpan? reconnectCheck = null)
        {
            reconnectCheck ??= TimeSpan.FromMinutes(5);

            while (true)
            {
                stoppingToken.ThrowIfCancellationRequested();

                var lastEventReceivedTime = DateTime.UtcNow;

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                var cancellationToken = cts.Token;

                using var timer = new Timer(_ =>
                {
                    if (cts.IsCancellationRequested)
                        return;

                    var timeSinceLastEvent = DateTime.UtcNow - lastEventReceivedTime;
                    if (timeSinceLastEvent > reconnectCheck.Value)
                    {
                        _logger.RestartingWatcher(lastEventReceivedTime);
                        cts.Cancel();
                    }
                },
                state: null,
                dueTime: reconnectCheck.Value / 2,
                period: reconnectCheck.Value / 2);

                IAsyncEnumerator<(WatchEventType, V1Pod)> watcher;

                try
                {
                    watcher = WatchPodAsyncCore(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    continue;
                }

                while (true)
                {
                    try
                    {
                        if (!await watcher.MoveNextAsync())
                            break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    lastEventReceivedTime = DateTime.UtcNow;
                    yield return watcher.Current;
                }
            }
        }

        private IAsyncEnumerator<(WatchEventType, V1Pod)> WatchPodAsyncCore(CancellationToken cancellationToken)
        {
            var watcher = KubernetesClient.CoreV1.ListNamespacedPodWithHttpMessagesAsync(
                namespaceParameter: PodNamespace,
                fieldSelector: $"metadata.name={PodName}",
                watch: true,
                cancellationToken: cancellationToken
            ).WatchAsync<V1Pod, V1PodList>(
                onError: ex =>
                {
                    if (ex is KubernetesException kubernetesError &&
                        string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.Ordinal))
                    {
                        throw ex;
                    }

                    _logger.IgnoringWatchingPodError(ex);
                },
                cancellationToken: cancellationToken
            );

            return watcher.GetAsyncEnumerator(cancellationToken);
        }

        public async Task PatchPodAsync(Action<IPodPatcherBuilder> configureBuilder)
        {
            var builder = PodPatcherBuilder.Create();
            configureBuilder.Invoke(builder);
            await PatchPodAsync(builder.Build());
        }

        private async Task PatchPodAsync(V1Patch patch)
        {
            try
            {
                await KubernetesClient.CoreV1.PatchNamespacedPodAsync(patch, PodName, PodNamespace);
            }
            catch (Exception ex)
            {
                _logger.PatchingPodError(JsonSerializer.Serialize(patch.Content), ex);
                throw;
            }
        }
    }
}
