using LiveStreamingServer.Rtmp.Core.RtmpMessages;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler
{
    public class RtmpHandshakeC0RequestHandler : IRequestHandler<RtmpHandshakeC0Request, bool>
    {
        public async Task<bool> Handle(RtmpHandshakeC0Request request, CancellationToken cancellationToken)
        {
            var payload = new byte[1];
            await request.NetworkStream.ReadExactlyAsync(payload, 0, 1, cancellationToken);
            request.PeerContext.State = RtmpClientPeerState.HandshakeC1;
            return true;
        }
    }
}
