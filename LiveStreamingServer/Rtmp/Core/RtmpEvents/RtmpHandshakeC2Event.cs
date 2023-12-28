using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core.RtmpEvents
{
    public record struct RtmpHandshakeC2Event(
        IRtmpClientPeerContext PeerContext,
        ReadOnlyNetworkStream NetworkStream) : IRequest<bool>;
}
