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
            var watcher = _context.WatchPodAsync(stoppingToken);

            await foreach (var (eventType, pod) in watcher)
            {
                if (eventType != WatchEventType.Added && eventType != WatchEventType.Modified)
                    continue;

                var labels = pod.Labels() ?? new Dictionary<string, string>();
                var annotataions = pod.Annotations() ?? new Dictionary<string, string>();

                await _lifetimeManager.ReconcileAsync(labels.AsReadOnly(), annotataions.AsReadOnly());
            }
        }
    }
}
