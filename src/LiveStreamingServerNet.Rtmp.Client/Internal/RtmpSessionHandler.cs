using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpSessionHandler : ISessionHandler
    {
        private readonly IRtmpSessionContext _context;
        private readonly IRtmpClientContext _clientContext;
        private readonly IMediator _mediator;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly ILogger _logger;
        private readonly IPool<RtmpChunkEvent> _rtmpChunkEventPool;

        public RtmpSessionHandler(
            IRtmpSessionContext context,
            IRtmpClientContext clientContext,
            IMediator mediator,
            IDataBufferPool dataBufferPool,
            ILogger<RtmpSessionHandler> logger)
        {
            _context = context;
            _clientContext = clientContext;
            _mediator = mediator;
            _dataBufferPool = dataBufferPool;
            _logger = logger;
            _rtmpChunkEventPool = new Pool<RtmpChunkEvent>(() => new RtmpChunkEvent());
        }

        public async ValueTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            _clientContext.SessionContext = _context;
            return await InitiateHandshakeAsync(_context, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> HandleSessionLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            try
            {
                var result = _context.State switch
                {
                    RtmpSessionState.HandshakeS0 => await HandleHandshakeS0Async(_context, networkStream, cancellationToken).ConfigureAwait(false),
                    RtmpSessionState.HandshakeS1 => await HandleHandshakeS1Async(_context, networkStream, cancellationToken).ConfigureAwait(false),
                    RtmpSessionState.HandshakeS2 => await HandleHandshakeS2Async(_context, networkStream, cancellationToken).ConfigureAwait(false),
                    _ => await HandleChunkAsync(_context, networkStream, cancellationToken).ConfigureAwait(false),
                };

                return result.Succeeded;
            }
            catch (Exception ex)
            when (ex is IOException || (ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.SessionLoopError(_context.Session.Id, ex);
                return false;
            }
        }

        private async ValueTask<bool> InitiateHandshakeAsync(IRtmpSessionContext context, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeInitiationEvent(context), cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeS0Async(IRtmpSessionContext context, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeS0Event(context, networkStream), cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeS1Async(IRtmpSessionContext context, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeS1Event(context, networkStream), cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeS2Async(IRtmpSessionContext context, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeS2Event(context, networkStream), cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleChunkAsync(IRtmpSessionContext context, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            var @event = _rtmpChunkEventPool.Obtain();

            try
            {
                @event.Context = context;
                @event.NetworkStream = networkStream;

                return await _mediator.Send(@event, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _rtmpChunkEventPool.Recycle(@event);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _context.Recycle(_dataBufferPool);
            await _context.DisposeAsync().ConfigureAwait(false);
        }
    }
}