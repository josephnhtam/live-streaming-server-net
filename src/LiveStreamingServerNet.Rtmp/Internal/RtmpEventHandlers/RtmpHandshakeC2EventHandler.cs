using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeC2EventHandler : IRequestHandler<RtmpHandshakeC2Event, bool>
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;

        public RtmpHandshakeC2EventHandler(
            INetBufferPool netBufferPool,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            ILogger<RtmpHandshakeC2EventHandler> logger)
        {
            _netBufferPool = netBufferPool;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        // todo: add validation
        public async Task<bool> Handle(RtmpHandshakeC2Event @event, CancellationToken cancellationToken)
        {
            using var incomingBuffer = _netBufferPool.Obtain();
            await incomingBuffer.CopyStreamData(@event.NetworkStream, 1536, cancellationToken);

            @event.ClientContext.State = RtmpClientState.HandshakeDone;

            _logger.HandshakeC2Handled(@event.ClientContext.Client.ClientId);

            await _eventDispatcher.RtmpClientHandshakeCompleteAsync(@event.ClientContext);

            return true;
        }
    }
}
