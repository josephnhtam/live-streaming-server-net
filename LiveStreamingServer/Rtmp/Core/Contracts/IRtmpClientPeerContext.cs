using LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.Handshakes;

namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpClientPeerContext
    {
        RtmpClientPeerState State { get; set; }
        HandshakeType HandshakeType { get; set; }
    }
}
