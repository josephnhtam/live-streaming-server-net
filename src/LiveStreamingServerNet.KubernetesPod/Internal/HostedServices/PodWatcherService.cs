using k8s;
using k8s.Models;
using LiveStreamingServerNet.KubernetesPod.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.KubernetesPod.Internal.HostedServices
{
    internal class PodWatcherService : BackgroundService
    {
        private readonly IKubernetesContext _context;
        private readonly IPodLifetimeManager _lifetimeManager;
        private readonly ILogger _logger;

        private readonly object syncLock = new();
        private Task? reconilationTask;

        public PodWatcherService(IKubernetesContext context, IPodLifetimeManager lifetimeManager, ILogger<PodWatcherService> logger)
        {
            _context = context;
            _lifetimeManager = lifetimeManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await WatchPod(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.WatchingPodError(ex);
                throw;
            }
        }

        private async Task WatchPod(CancellationToken stoppingToken)
        {
            await _context.WatchPodAsync(
                (eventType, pod) =>
                {
                    if (eventType != WatchEventType.Added && eventType != WatchEventType.Modified)
                        return;

                    var labels = pod.Labels() ?? new Dictionary<string, string>();
                    var annotataions = pod.Annotations() ?? new Dictionary<string, string>();

                    lock (syncLock)
                    {
                        if (reconilationTask == null || reconilationTask.IsCompleted)
                        {
                            reconilationTask = _lifetimeManager.ReconcileAsync(labels.AsReadOnly(), annotataions.AsReadOnly(), stoppingToken).AsTask();
                        }
                        else
                        {
                            reconilationTask = reconilationTask.ContinueWith(async _ =>
                                await _lifetimeManager.ReconcileAsync(labels.AsReadOnly(), annotataions.AsReadOnly(), stoppingToken),
                                TaskContinuationOptions.ExecuteSynchronously
                            );
                        }
                    }
                },
                stoppingToken: stoppingToken
            );

            if (reconilationTask != null && !reconilationTask.IsCompleted)
                await reconilationTask;
        }
    }
}
