using k8s;
using k8s.Models;
using LiveStreamingServerNet.KubernetesPod.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.KubernetesPod.Utilities;
using LiveStreamingServerNet.KubernetesPod.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LiveStreamingServerNet.Utilities.Common;

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

        public async Task WatchPodAsync(Action<WatchEventType, V1Pod> onPodEvent, WatchPodOptions? options = null, CancellationToken stoppingToken = default)
        {
            var reconnectCheck = options?.ReconnectCheck ?? TimeSpan.FromMinutes(5);
            var retryDelay = options?.ReconnectCheck ?? TimeSpan.FromSeconds(5);

            while (true)
            {
                stoppingToken.ThrowIfCancellationRequested();

                try
                {
                    var lastEventReceivedTime = DateTime.UtcNow;

                    using var watcherCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    var watcherCancellation = watcherCts.Token;

                    var watcherCompletionSource = new TaskCompletionSource();

                    _logger.StartingPodWatcher();

                    using var watcher = CreatePodWatcher(
                        (eventType, pod) =>
                        {
                            _logger.PodEventReceived(eventType);

                            lastEventReceivedTime = DateTime.UtcNow;
                            onPodEvent(eventType, pod);
                        },
                        watcherCompletionSource.SetException,
                        watcherCompletionSource.SetResult,
                        watcherCancellation);

                    using var timer = new Timer(_ =>
                    {
                        if (watcherCancellation.IsCancellationRequested)
                            return;

                        var timeSinceLastEvent = DateTime.UtcNow - lastEventReceivedTime;
                        if (timeSinceLastEvent > reconnectCheck)
                        {
                            _logger.RestartingPodWatcher(lastEventReceivedTime);
                            ErrorBoundary.Execute(watcherCts.Cancel);
                            ErrorBoundary.Execute(watcher.Dispose);
                        }
                    },
                    state: null,
                    dueTime: reconnectCheck / 2,
                    period: reconnectCheck / 2);

                    await watcherCompletionSource.Task;
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.WatchingPodError(ex, retryDelay);
                    await Task.Delay(retryDelay, stoppingToken);
                }
            }
        }

        private Watcher<V1Pod> CreatePodWatcher(Action<WatchEventType, V1Pod> onPodEvent, Action<Exception> onError, Action onClosed, CancellationToken cancellationToken)
        {
            return KubernetesClient.CoreV1.ListNamespacedPodWithHttpMessagesAsync(
                namespaceParameter: PodNamespace,
                fieldSelector: $"metadata.name={PodName}",
                watch: true,
                cancellationToken: cancellationToken
            ).Watch(
                onEvent: onPodEvent,
                onError: ex =>
                {
                    if (ex is KubernetesException kubernetesError &&
                        string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.OrdinalIgnoreCase))
                    {
                        onError(ex);
                        throw ex;
                    }

                    _logger.IgnoringWatchingPodError(ex);
                },
                onClosed: onClosed
            );
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
