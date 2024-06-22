using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientHandler : IRtmpClientHandler
    {
        private readonly IMediator _mediator;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly IRtmpClientContextFactory _clientContextFactory;
        private readonly ILogger _logger;
        private readonly IBandwidthLimiter? _bandwidthLimiter;
        private readonly IPool<RtmpChunkEvent> _rtmpChunkEventPool;

        private IRtmpClientContext _clientContext = default!;

        public RtmpClientHandler(
            IMediator mediator,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            IRtmpClientContextFactory clientContextFactory,
            ILogger<RtmpClientHandler> logger,
            IBandwidthLimiterFactory? bandwidthLimiterFactory = null)
        {
            _mediator = mediator;
            _eventDispatcher = eventDispatcher;
            _clientContextFactory = clientContextFactory;
            _logger = logger;
            _bandwidthLimiter = bandwidthLimiterFactory?.Create();
            _rtmpChunkEventPool = new Pool<RtmpChunkEvent>(() => new RtmpChunkEvent());
        }

        public async Task InitializeAsync(IClientHandle client)
        {
            _clientContext = _clientContextFactory.Create(client);
            await OnRtmpClientCreatedAsync();
        }

        public async Task<bool> HandleClientLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            try
            {
                var result = _clientContext.State switch
                {
                    RtmpClientState.HandshakeC0 => await HandleHandshakeC0(_clientContext, networkStream, cancellationToken),
                    RtmpClientState.HandshakeC1 => await HandleHandshakeC1(_clientContext, networkStream, cancellationToken),
                    RtmpClientState.HandshakeC2 => await HandleHandshakeC2(_clientContext, networkStream, cancellationToken),
                    _ => await HandleChunkAsync(_clientContext, networkStream, cancellationToken),
                };

                if (result.Succeeded && _bandwidthLimiter != null && !_bandwidthLimiter.ConsumeBandwidth(result.ConsumedBytes))
                {
                    _logger.ExceededBandwidthLimit(_clientContext.Client.ClientId);
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
                _logger.ClientLoopError(_clientContext.Client.ClientId, ex);
                return false;
            }
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeC0(IRtmpClientContext clientContext, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC0Event(clientContext, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeC1(IRtmpClientContext clientContext, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC1Event(clientContext, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeC2(IRtmpClientContext clientContext, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC2Event(clientContext, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleChunkAsync(IRtmpClientContext clientContext, INetworkStreamReader networkStream, CancellationToken cancellationToken)
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

            await _clientContext.DisposeAsync();
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
