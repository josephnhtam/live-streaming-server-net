using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.RtmpMessages;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler
{
    public class RtmpHandshakeC2RequestHandler : IRequestHandler<RtmpHandshakeC2Request, bool>
    {
        // todo: add validation
        public async Task<bool> Handle(RtmpHandshakeC2Request request, CancellationToken cancellationToken)
        {
            var incomingBuffer = new NetBuffer(1536);
            await incomingBuffer.ReadFromAsync(request.NetworkStream, 1536, cancellationToken);

            request.PeerContext.State = RtmpClientPeerState.HandshakeDone;

            return true;
        }
    }
}
