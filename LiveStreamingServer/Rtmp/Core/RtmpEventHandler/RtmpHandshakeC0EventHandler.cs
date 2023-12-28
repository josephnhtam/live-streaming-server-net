using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler
{
    public class RtmpHandshakeC0EventHandler : IRequestHandler<RtmpHandshakeC0Event, bool>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeC0EventHandler(ILogger<RtmpHandshakeC0EventHandler> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpHandshakeC0Event @event, CancellationToken cancellationToken)
        {
            var payload = new byte[1];
            await @event.NetworkStream.ReadExactlyAsync(payload, 0, 1, cancellationToken);

            @event.PeerContext.State = RtmpClientPeerState.HandshakeC1;

            _logger.LogDebug("PeerId: {PeerId} | Handshake C0 Handled", @event.PeerContext.Peer.PeerId);

            return true;
        }
    }
}
