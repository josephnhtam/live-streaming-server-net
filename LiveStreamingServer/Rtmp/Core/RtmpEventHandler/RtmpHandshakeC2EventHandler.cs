using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler
{
    public class RtmpHandshakeC2EventHandler : IRequestHandler<RtmpHandshakeC2Event, bool>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeC2EventHandler(ILogger<RtmpHandshakeC2EventHandler> logger)
        {
            _logger = logger;
        }

        // todo: add validation
        public async Task<bool> Handle(RtmpHandshakeC2Event @event, CancellationToken cancellationToken)
        {
            var incomingBuffer = new NetBuffer(1536);
            await incomingBuffer.CopyStreamData(@event.NetworkStream, 1536, cancellationToken);

            @event.PeerContext.State = RtmpClientPeerState.HandshakeDone;

            _logger.LogDebug("PeerId: {PeerId} | Handshake C2 Handled", @event.PeerContext.Peer.PeerId);

            return true;
        }
    }
}
