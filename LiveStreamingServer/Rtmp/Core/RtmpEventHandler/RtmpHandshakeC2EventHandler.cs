using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler
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

            _logger.LogDebug("PeerId: {PeerId} | Handshake C2 Handled", @event.PeerContext.Peer.PeerId);

            return true;
        }
    }
}
