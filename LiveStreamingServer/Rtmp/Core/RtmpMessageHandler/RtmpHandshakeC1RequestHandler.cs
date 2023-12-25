using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.Handshakes;
using LiveStreamingServer.Rtmp.Core.RtmpMessages;
using LiveStreamingServer.Rtmp.Core.Utilities;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler
{
    public class RtmpHandshakeC1RequestHandler : IRequestHandler<RtmpHandshakeC1Request, bool>
    {
        public async Task<bool> Handle(RtmpHandshakeC1Request request, CancellationToken cancellationToken)
        {
            var incomingBuffer = new NetBuffer(1536);
            await incomingBuffer.ReadFromAsync(request.NetworkStream, 1536, cancellationToken);

            var outgoingBuffer = new NetBuffer(1536);
            if (HandleHandshake(request.PeerContext, incomingBuffer, outgoingBuffer))
            {
                request.PeerContext.State = RtmpClientPeerState.HandshakeC2;
                request.ClientPeer.Send(outgoingBuffer);
                return true;
            }

            return false;
        }

        private bool HandleHandshake(IRtmpClientPeerContext peerContext, NetBuffer incomingBuffer, NetBuffer outgoingBuffer)
        {
            var complexHandshake0 = new ComplexHandshake(incomingBuffer, ComplexHandshakeType.Schema0);
            if (complexHandshake0.ValidateC1())
            {
                peerContext.HandshakeType = HandshakeType.ComplexHandshake0;
                complexHandshake0.WriteS0S1S2(outgoingBuffer);
                return true;
            }

            var complexHandshake1 = new ComplexHandshake(incomingBuffer, ComplexHandshakeType.Schema1);
            if (complexHandshake1.ValidateC1())
            {
                peerContext.HandshakeType = HandshakeType.ComplexHandshake1;
                complexHandshake1.WriteS0S1S2(outgoingBuffer);
                return true;
            }

            var simpleHandshake = new SimpleHandshake(incomingBuffer);
            if (simpleHandshake.ValidateC1())
            {
                peerContext.HandshakeType = HandshakeType.SimpleHandshake;
                simpleHandshake.WriteS0S1S2(outgoingBuffer);
                return true;
            }

            return false;
        }
    }
}
