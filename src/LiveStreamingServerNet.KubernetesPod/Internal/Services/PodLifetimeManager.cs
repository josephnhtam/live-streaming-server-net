using LiveStreamingServerNet.KubernetesPod.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services
{
    internal class PodLifetimeManager : IPodLifetimeManager
    {
        private readonly IKubernetesContext _kubernetesContext;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ILogger _logger;

        private int _streamsCount;
        private int _streamsLimit;
        private bool _isPendingStop;

        public int StreamsCount => _streamsCount;
        public int StreamsLimit => _streamsLimit;
        public bool IsPendingStop => _isPendingStop;
        public bool IsStreamsLimitReached => _streamsCount >= _streamsLimit;

        public PodLifetimeManager(
            IKubernetesContext kubernetesContext,
            IHostApplicationLifetime appLifetime,
            ILogger<PodLifetimeManager> logger)
        {
            _kubernetesContext = kubernetesContext;
            _appLifetime = appLifetime;
            _logger = logger;

            if (!int.TryParse(Environment.GetEnvironmentVariable(PodConstants.StreamsLimitEnv), out _streamsLimit))
                _streamsLimit = int.MaxValue;
        }

        public async ValueTask ReconcileAsync(IReadOnlyDictionary<string, string> labels, IReadOnlyDictionary<string, string> annotations)
        {
            if (!labels.TryGetValue(PodConstants.PendingStopLabel, out var isPendingStopStr))
            {
                _isPendingStop = false;
                return;
            }

            _isPendingStop = bool.TryParse(isPendingStopStr, out var result) && result;
            await StopPodIfConditionMetAsync();
        }

        private async Task UpdatePodAsync()
        {
            await _kubernetesContext.PatchPodAsync(builder =>
                builder.SetAnnotation(PodConstants.StreamsCountAnnotation, _streamsCount.ToString())
                       .SetLabel(PodConstants.StreamsLimitReachedLabel, IsStreamsLimitReached.ToString().ToLower())
            );
        }

        private ValueTask StopPodIfConditionMetAsync()
        {
            if (!_isPendingStop || _streamsCount > 0 ||
                _appLifetime.ApplicationStopping.IsCancellationRequested ||
                _appLifetime.ApplicationStopped.IsCancellationRequested)
                return ValueTask.CompletedTask;

            _logger.StoppingPodGracefully();
            _appLifetime.StopApplication();

            return ValueTask.CompletedTask;
        }

        public async ValueTask OnClientDisposedAsync(uint clientId)
        {
            await StopPodIfConditionMetAsync();
        }

        public async ValueTask OnStreamPublishedAsync(uint clientId, string streamIdentifier)
        {
            Interlocked.Increment(ref _streamsCount);
            await UpdatePodAsync();
        }

        public async ValueTask OnStreamUnpublishedAsync(uint clientId, string streamIdentifier)
        {
            Interlocked.Decrement(ref _streamsCount);
            await UpdatePodAsync();
        }
    }
}
