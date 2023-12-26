using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.Handshakes;
using LiveStreamingServer.Rtmp.Core.RtmpMessages;
using LiveStreamingServer.Rtmp.Core.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler
{
    public class RtmpHandshakeC1MessageHandler : IRequestHandler<RtmpHandshakeC1Message, bool>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeC1MessageHandler(ILogger<RtmpHandshakeC1MessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpHandshakeC1Message message, CancellationToken cancellationToken)
        {
            var incomingBuffer = new NetBuffer(1536);
            await incomingBuffer.CopyStreamData(message.NetworkStream, 1536, cancellationToken);

            var outgoingBuffer = new NetBuffer(1536);
            if (HandleHandshake(message, incomingBuffer, outgoingBuffer))
            {
                message.PeerContext.State = RtmpClientPeerState.HandshakeC2;
                message.ClientPeer.Send(outgoingBuffer);

                _logger.LogDebug("PeerId: {PeerId} | C1 Handled", message.ClientPeer.PeerId);

                return true;
            }

            _logger.LogDebug("PeerId: {PeerId} | C1 Handling Failed", message.ClientPeer.PeerId);

            return false;
        }

        private bool HandleHandshake(RtmpHandshakeC1Message request, NetBuffer incomingBuffer, NetBuffer outgoingBuffer)
        {
            var clientPeer = request.ClientPeer;
            var peerContext = request.PeerContext;

            var complexHandshake0 = new ComplexHandshake(incomingBuffer, ComplexHandshakeType.Schema0);
            if (complexHandshake0.ValidateC1())
            {
                peerContext.HandshakeType = HandshakeType.ComplexHandshake0;
                complexHandshake0.WriteS0S1S2(outgoingBuffer);
                _logger.LogDebug("PeerId: {PeerId} | Handshake type: {HandshakeType}", clientPeer.PeerId, nameof(HandshakeType.ComplexHandshake0));
                return true;
            }

            var complexHandshake1 = new ComplexHandshake(incomingBuffer, ComplexHandshakeType.Schema1);
            if (complexHandshake1.ValidateC1())
            {
                peerContext.HandshakeType = HandshakeType.ComplexHandshake1;
                complexHandshake1.WriteS0S1S2(outgoingBuffer);
                _logger.LogDebug("PeerId: {PeerId} | Handshake type: {HandshakeType}", clientPeer.PeerId, nameof(HandshakeType.ComplexHandshake1));
                return true;
            }

            var simpleHandshake = new SimpleHandshake(incomingBuffer);
            if (simpleHandshake.ValidateC1())
            {
                peerContext.HandshakeType = HandshakeType.SimpleHandshake;
                simpleHandshake.WriteS0S1S2(outgoingBuffer);
                _logger.LogDebug("PeerId: {PeerId} | Handshake type: {HandshakeType}", clientPeer.PeerId, nameof(HandshakeType.SimpleHandshake));
                return true;
            }

            return false;
        }
    }
}
