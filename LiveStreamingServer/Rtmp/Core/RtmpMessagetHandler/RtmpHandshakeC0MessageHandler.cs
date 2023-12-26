using LiveStreamingServer.Rtmp.Core.RtmpMessages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler
{
    public class RtmpHandshakeC0MessageHandler : IRequestHandler<RtmpHandshakeC0Message, bool>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeC0MessageHandler(ILogger<RtmpHandshakeC0MessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpHandshakeC0Message request, CancellationToken cancellationToken)
        {
            var payload = new byte[1];
            await request.NetworkStream.ReadExactlyAsync(payload, 0, 1, cancellationToken);

            request.PeerContext.State = RtmpClientPeerState.HandshakeC1;

            _logger.LogDebug("PeerId: {PeerId} | C0 Handled", request.ClientPeer.PeerId);

            return true;
        }
    }
}
