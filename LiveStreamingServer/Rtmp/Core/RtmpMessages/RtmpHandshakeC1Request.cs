using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessages
{
    public record struct RtmpHandshakeC1Request(
        IClientPeerHandle ClientPeer,
        IRtmpClientPeerContext PeerContext,
        ReadOnlyNetworkStream NetworkStream) : IRequest<bool>;
}
