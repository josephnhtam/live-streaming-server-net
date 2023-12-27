using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core.RtmpEvents
{
    public record struct RtmpHandshakeC0Event(
        IClientPeerHandle ClientPeer,
        IRtmpClientPeerContext PeerContext,
        ReadOnlyNetworkStream NetworkStream) : IRequest<bool>;
}
