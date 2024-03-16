using LiveStreamingServerNet.KubernetesPod.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services
{
    internal class PodLifetimeManager : IPodLifetimeManager
    {
        private const string IsPendingStopLabel = "pending-stop";
        private const string StreamCountAnnotation = "stream-count";

        private readonly IKubernetesContext _kubernetesContext;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ILogger _logger;

        private int _streamCount;
        private bool _isPendingStop;

        public int StreamCount => _streamCount;
        public bool IsPendingStop => _isPendingStop;

        public PodLifetimeManager(IKubernetesContext kubernetesContext, IHostApplicationLifetime appLifetime, ILogger<PodLifetimeManager> logger)
        {
            _kubernetesContext = kubernetesContext;
            _appLifetime = appLifetime;
            _logger = logger;
        }

        public async ValueTask ReconcileAsync(IDictionary<string, string> labels, IDictionary<string, string> annotations)
        {
            if (!labels.TryGetValue(IsPendingStopLabel, out var isPendingStopStr))
            {
                _isPendingStop = false;
                return;
            }

            _isPendingStop = bool.TryParse(isPendingStopStr, out var result) && result;
            await StopPodIfConditionMetAsync();
        }

        public async ValueTask OnRtmpClientDisposedAsync(uint clientId)
        {
            await StopPodIfConditionMetAsync();
        }

        public async ValueTask OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            Interlocked.Increment(ref _streamCount);
            await UpdatePodAsync();
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            Interlocked.Decrement(ref _streamCount);
            await UpdatePodAsync();
        }

        private async Task UpdatePodAsync()
        {
            await _kubernetesContext.PatchPodAsync(builder =>
                builder.SetAnnotation(StreamCountAnnotation, _streamCount.ToString())
            );
        }

        private ValueTask StopPodIfConditionMetAsync()
        {
            if (!_isPendingStop || _streamCount > 0 ||
                _appLifetime.ApplicationStopping.IsCancellationRequested ||
                _appLifetime.ApplicationStopped.IsCancellationRequested)
                return ValueTask.CompletedTask;

            _logger.StoppingPodGracefully();
            _appLifetime.StopApplication();

            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpClientCreatedAsync(IClientControl client)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpClientConnectedAsync(
            uint clientId,
            IReadOnlyDictionary<string, object> commandObject,
            IReadOnlyDictionary<string, object>? arguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpClientHandshakeCompleteAsync(uint clientId)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
            => ValueTask.CompletedTask;
    }
}
