using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerContext : IRtmpClientPeerContext
    {
        public RtmpClientPeerState State { get; set; } = RtmpClientPeerState.BeforeHandshake;
    }
}
