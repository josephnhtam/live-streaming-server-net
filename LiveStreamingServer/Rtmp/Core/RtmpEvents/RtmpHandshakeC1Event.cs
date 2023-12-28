using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core.RtmpEvents
{
    public record struct RtmpHandshakeC1Event(
        IRtmpClientPeerContext PeerContext,
        ReadOnlyNetworkStream NetworkStream) : IRequest<bool>;
}
