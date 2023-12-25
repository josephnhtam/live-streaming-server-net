using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Utilities;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerContext : IRtmpClientPeerContext
    {
        public RtmpClientPeerState State { get; set; } = RtmpClientPeerState.HandshakeC0;
        public HandshakeType HandshakeType { get; set; } = HandshakeType.SimpleHandshake;
    }
}
