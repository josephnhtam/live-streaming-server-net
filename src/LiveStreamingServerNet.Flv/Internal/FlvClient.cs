using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvClient : IFlvClient
    {
        public string ClientId { get; }
        public string StreamPath { get; }
        public CancellationToken StoppingToken { get; }

        private readonly ILogger _logger;
        private readonly IFlvWriter _flvWriter;
        private readonly IFlvMediaTagBroadcasterService _mediaTagBroadcaster;
        private readonly TaskCompletionSource _initializationTcs = new();

        private readonly CancellationTokenSource _stoppingCts;
        private readonly TaskCompletionSource _taskCompletionSource;
        private readonly Task _initializationTask;
        private readonly Task _completeTask;

        private bool _isDisposed;

        public FlvClient(
            string clientId,
            string streamPath,
            IFlvMediaTagBroadcasterService mediaTagBroadcaster,
            IFlvWriter flvWriter,
            ILogger<FlvClient> logger,
            CancellationToken stoppingToken)
        {
            _mediaTagBroadcaster = mediaTagBroadcaster;
            _flvWriter = flvWriter;
            _logger = logger;

            ClientId = clientId;
            StreamPath = streamPath;

            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            StoppingToken = _stoppingCts.Token;

            _taskCompletionSource = new TaskCompletionSource();

            _stoppingCts.Token.Register(() =>
            {
                _initializationTcs.TrySetCanceled();
                _taskCompletionSource.TrySetResult();
            });

            _initializationTask = _initializationTcs.Task;
            _completeTask = _taskCompletionSource.Task;

            _mediaTagBroadcaster.RegisterClient(this);
        }

        public void CompleteInitialization()
        {
            _initializationTcs.SetResult();
        }

        public Task UntilInitializationComplete()
        {
            return _initializationTask;
        }

        public Task UntilComplete()
        {
            return _completeTask;
        }

        public void Stop()
        {
            _stoppingCts.Cancel();
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _mediaTagBroadcaster.UnregisterClient(this);
            _stoppingCts.Cancel();
            _stoppingCts.Dispose();
            await _flvWriter.DisposeAsync();

            GC.SuppressFinalize(this);
        }

        public async ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken)
        {
            try
            {
                await _flvWriter.WriteHeaderAsync(allowAudioTags, allowVideoTags, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.FailedToWriteFlvHeader(ClientId, ex);
                Stop();
            }
        }

        public async ValueTask WriteTagAsync(FlvTagType tagType, uint timestamp, Action<IDataBuffer> payloadBuffer, CancellationToken cancellationToken)
        {
            try
            {
                await _flvWriter.WriteTagAsync(tagType, timestamp, payloadBuffer, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.FailedToWriteFlvTag(ClientId, ex);
                Stop();
            }
        }
    }
}
