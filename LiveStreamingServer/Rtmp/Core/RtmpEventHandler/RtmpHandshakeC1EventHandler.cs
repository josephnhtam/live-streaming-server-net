using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Handshakes;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using LiveStreamingServer.Rtmp.Core.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler
{
    public class RtmpHandshakeC1EventHandler : IRequestHandler<RtmpHandshakeC1Event, bool>
    {
        private readonly ILogger _logger;

        public RtmpHandshakeC1EventHandler(ILogger<RtmpHandshakeC1EventHandler> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpHandshakeC1Event @event, CancellationToken cancellationToken)
        {
            var incomingBuffer = new NetBuffer(1536);
            await incomingBuffer.CopyStreamData(@event.NetworkStream, 1536, cancellationToken);

            var outgoingBuffer = new NetBuffer(1536);
            if (HandleHandshake(@event, incomingBuffer, outgoingBuffer))
            {
                @event.PeerContext.State = RtmpClientPeerState.HandshakeC2;
                @event.ClientPeer.Send(outgoingBuffer);

                _logger.LogDebug("PeerId: {PeerId} | C1 Handled", @event.ClientPeer.PeerId);

                return true;
            }

            _logger.LogDebug("PeerId: {PeerId} | C1 Handling Failed", @event.ClientPeer.PeerId);

            return false;
        }

        private bool HandleHandshake(RtmpHandshakeC1Event @event, NetBuffer incomingBuffer, NetBuffer outgoingBuffer)
        {
            var clientPeer = @event.ClientPeer;
            var peerContext = @event.PeerContext;

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
