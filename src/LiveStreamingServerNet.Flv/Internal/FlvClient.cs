using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Utilities.Containers;
using LiveStreamingServerNet.Rtmp.Utilities.Containers.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvClient : IFlvClient
    {
        public string ClientId { get; }
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }
        public IFlvRequest Request { get; }
        public CancellationToken StoppingToken { get; }

        private readonly ILogger _logger;
        private readonly IFlvWriter _flvWriter;
        private readonly IFlvMediaTagBroadcasterService _mediaTagBroadcaster;
        private readonly TaskCompletionSource _initializationTcs = new();

        private readonly CancellationTokenSource _stoppingCts;
        private readonly Task _initializationTask;
        private readonly Task _completeTask;

        private bool _isDisposed;

        public FlvClient(
            string clientId,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            IFlvRequest request,
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
            StreamArguments = new Dictionary<string, string>(streamArguments);
            Request = request;

            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            StoppingToken = _stoppingCts.Token;

            var taskCompletionSource = new TaskCompletionSource();

            _stoppingCts.Token.Register(() =>
            {
                _initializationTcs.TrySetCanceled();
                taskCompletionSource.TrySetResult();
            });

            _initializationTask = _initializationTcs.Task;
            _completeTask = taskCompletionSource.Task;

            _mediaTagBroadcaster.RegisterClient(this);
        }

        public void CompleteInitialization()
        {
            _initializationTcs.TrySetResult();
        }

        public Task UntilInitializationCompleteAsync(CancellationToken cancellationToken)
        {
            return _initializationTask.WithCancellation(cancellationToken);
        }

        public Task UntilCompleteAsync(CancellationToken cancellationToken)
        {
            return _completeTask.WithCancellation(cancellationToken);
        }

        public void Stop()
        {
            try
            {
                _stoppingCts.Cancel();
            }
            catch (ObjectDisposedException) { }
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            await _mediaTagBroadcaster.UnregisterClientAsync(this).ConfigureAwait(false);

            _stoppingCts.Cancel();
            _stoppingCts.Dispose();

            await _flvWriter.DisposeAsync().ConfigureAwait(false);
        }

        public async ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken)
        {
            try
            {
                await _flvWriter.WriteHeaderAsync(allowAudioTags, allowVideoTags, cancellationToken).ConfigureAwait(false);
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
                await _flvWriter.WriteTagAsync(tagType, timestamp, payloadBuffer, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.FailedToWriteFlvTag(ClientId, ex);
                Stop();
            }
        }
    }
}
