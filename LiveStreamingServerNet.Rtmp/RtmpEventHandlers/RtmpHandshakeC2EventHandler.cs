using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using LiveStreamingServerNet.Rtmp.RtmpEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers
{
    public class RtmpHandshakeC2EventHandler : IRequestHandler<RtmpHandshakeC2Event, bool>
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly ILogger _logger;

        public RtmpHandshakeC2EventHandler(INetBufferPool netBufferPool, ILogger<RtmpHandshakeC2EventHandler> logger)
        {
            _netBufferPool = netBufferPool;
            _logger = logger;
        }

        // todo: add validation
        public async Task<bool> Handle(RtmpHandshakeC2Event @event, CancellationToken cancellationToken)
        {
            using var incomingBuffer = _netBufferPool.Obtain();
            await incomingBuffer.CopyStreamData(@event.NetworkStream, 1536, cancellationToken);

            @event.PeerContext.State = RtmpClientPeerState.HandshakeDone;

            _logger.HandshakeC2Handled(@event.PeerContext.Peer.PeerId);

            return true;
        }
    }
}
