using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.Handshakes;
using LiveStreamingServerNet.Rtmp.RtmpEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler
{
    public class RtmpHandshakeC1EventHandler : IRequestHandler<RtmpHandshakeC1Event, bool>
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly ILogger _logger;

        public RtmpHandshakeC1EventHandler(INetBufferPool netBufferPool, ILogger<RtmpHandshakeC1EventHandler> logger)
        {
            _netBufferPool = netBufferPool;
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpHandshakeC1Event @event, CancellationToken cancellationToken)
        {
            using var incomingBuffer = _netBufferPool.Obtain();
            await incomingBuffer.CopyStreamData(@event.NetworkStream, 1536, cancellationToken);

            using var outgoingBuffer = _netBufferPool.Obtain();
            if (HandleHandshake(@event, incomingBuffer, outgoingBuffer))
            {
                @event.PeerContext.State = RtmpClientPeerState.HandshakeC2;
                @event.PeerContext.Peer.Send(outgoingBuffer);

                _logger.LogDebug("PeerId: {PeerId} | Handshake C1 Handled", @event.PeerContext.Peer.PeerId);

                return true;
            }

            _logger.LogDebug("PeerId: {PeerId} | Handshake C1 Handling Failed", @event.PeerContext.Peer.PeerId);

            return false;
        }

        private bool HandleHandshake(RtmpHandshakeC1Event @event, INetBuffer incomingBuffer, INetBuffer outgoingBuffer)
        {
            var peerContext = @event.PeerContext;
            var clientPeer = peerContext.Peer;

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
