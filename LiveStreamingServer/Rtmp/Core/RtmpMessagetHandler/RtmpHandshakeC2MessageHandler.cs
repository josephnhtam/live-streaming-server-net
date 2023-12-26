using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.RtmpMessages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler
{
    public class RtmpHandshakeC2MessageHandler : IRequestHandler<RtmpHandshakeC2Message, bool>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeC2MessageHandler(ILogger<RtmpHandshakeC2MessageHandler> logger)
        {
            _logger = logger;
        }

        // todo: add validation
        public async Task<bool> Handle(RtmpHandshakeC2Message request, CancellationToken cancellationToken)
        {
            var incomingBuffer = new NetBuffer(1536);
            await incomingBuffer.CopyStreamData(request.NetworkStream, 1536, cancellationToken);

            request.PeerContext.State = RtmpClientPeerState.HandshakeDone;

            _logger.LogDebug("PeerId: {PeerId} | C2 Handled", request.ClientPeer.PeerId);

            return true;
        }
    }
}
