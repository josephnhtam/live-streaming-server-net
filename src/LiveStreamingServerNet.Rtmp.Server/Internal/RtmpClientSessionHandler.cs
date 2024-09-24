using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Server.RateLimiting.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpClientSessionHandler : IRtmpClientSessionHandler
    {
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IMediator _mediator;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;
        private readonly IBandwidthLimiter? _bandwidthLimiter;
        private readonly IPool<RtmpChunkEvent> _rtmpChunkEventPool;

        public RtmpClientSessionHandler(
            IRtmpClientSessionContext clientContext,
            IMediator mediator,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            ILogger<RtmpClientSessionHandler> logger,
            IBandwidthLimiterFactory? bandwidthLimiterFactory = null)
        {
            _clientContext = clientContext;
            _mediator = mediator;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
            _bandwidthLimiter = bandwidthLimiterFactory?.Create();
            _rtmpChunkEventPool = new Pool<RtmpChunkEvent>(() => new RtmpChunkEvent());
        }

        public async ValueTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            await OnRtmpClientCreatedAsync();
            return true;
        }

        public async Task<bool> HandleSessionLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            try
            {
                var result = _clientContext.State switch
                {
                    RtmpClientSessionState.HandshakeC0 => await HandleHandshakeC0Async(_clientContext, networkStream, cancellationToken),
                    RtmpClientSessionState.HandshakeC1 => await HandleHandshakeC1Async(_clientContext, networkStream, cancellationToken),
                    RtmpClientSessionState.HandshakeC2 => await HandleHandshakeC2Async(_clientContext, networkStream, cancellationToken),
                    _ => await HandleChunkAsync(_clientContext, networkStream, cancellationToken),
                };

                if (result.Succeeded && _bandwidthLimiter != null && !_bandwidthLimiter.ConsumeBandwidth(result.ConsumedBytes))
                {
                    _logger.ExceededBandwidthLimit(_clientContext.Client.Id);
                    return false;
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            when (ex is IOException || (ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.ClientLoopError(_clientContext.Client.Id, ex);
                return false;
            }
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeC0Async(IRtmpClientSessionContext clientContext, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC0Event(clientContext, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeC1Async(IRtmpClientSessionContext clientContext, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC1Event(clientContext, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeC2Async(IRtmpClientSessionContext clientContext, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC2Event(clientContext, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleChunkAsync(IRtmpClientSessionContext clientContext, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            var @event = _rtmpChunkEventPool.Obtain();

            try
            {
                @event.ClientContext = clientContext;
                @event.NetworkStream = networkStream;

                return await _mediator.Send(@event, cancellationToken);
            }
            finally
            {
                _rtmpChunkEventPool.Recycle(@event);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await OnRtmpClientDisposedAsync();

            if (_bandwidthLimiter != null)
                await _bandwidthLimiter.DisposeAsync();

            _clientContext.Dispose();
        }

        private async ValueTask OnRtmpClientCreatedAsync()
        {
            await _eventDispatcher.RtmpClientCreatedAsync(_clientContext);
        }

        private async ValueTask OnRtmpClientDisposedAsync()
        {
            await _eventDispatcher.RtmpClientDisposedAsync(_clientContext);
        }
    }
}
