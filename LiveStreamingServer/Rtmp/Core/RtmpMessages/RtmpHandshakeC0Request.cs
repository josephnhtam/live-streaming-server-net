using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessages
{
    public record struct RtmpHandshakeC0Request(
        IClientPeer ClientPeer,
        IRtmpClientPeerContext PeerContext,
        ReadOnlyNetworkStream NetworkStream);
}
