using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpSessionHandler : ISessionHandler
    {
        private readonly IRtmpSessionContext _context;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IPool<RtmpChunkEvent> _rtmpChunkEventPool;

        public RtmpSessionHandler(
            IRtmpSessionContext context,
            IMediator mediator,
            ILogger<RtmpSessionHandler> logger)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
            _rtmpChunkEventPool = new Pool<RtmpChunkEvent>(() => new RtmpChunkEvent());
        }

        public async ValueTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            return await InitiateHandshakeAsync(_context, cancellationToken);
        }

        public async Task<bool> HandleSessionLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            try
            {
                var result = _context.State switch
                {
                    RtmpSessionState.HandshakeS0 => await HandleHandshakeS0Async(_context, networkStream, cancellationToken),
                    RtmpSessionState.HandshakeS1 => await HandleHandshakeS1Async(_context, networkStream, cancellationToken),
                    RtmpSessionState.HandshakeS2 => await HandleHandshakeS2Async(_context, networkStream, cancellationToken),
                    _ => await HandleChunkAsync(_context, networkStream, cancellationToken),
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
            return await _mediator.Send(new RtmpHandshakeInitiationEvent(context), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeS0Async(IRtmpSessionContext context, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeS0Event(context, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeS1Async(IRtmpSessionContext context, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeS1Event(context, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleHandshakeS2Async(IRtmpSessionContext context, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeS2Event(context, networkStream), cancellationToken);
        }

        private async ValueTask<RtmpEventConsumingResult> HandleChunkAsync(IRtmpSessionContext context, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            var @event = _rtmpChunkEventPool.Obtain();

            try
            {
                @event.Context = context;
                @event.NetworkStream = networkStream;

                //return await _mediator.Send(@event, cancellationToken);
                await Task.Yield();
                return new RtmpEventConsumingResult(true, 0);
            }
            finally
            {
                _rtmpChunkEventPool.Recycle(@event);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}