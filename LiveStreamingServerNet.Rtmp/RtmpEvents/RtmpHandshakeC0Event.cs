using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Rtmp.Contracts;
using MediatR;

namespace LiveStreamingServerNet.Rtmp.RtmpEvents
{
    internal record struct RtmpHandshakeC0Event(
        IRtmpClientPeerContext PeerContext,
        ReadOnlyNetworkStream NetworkStream) : IRequest<bool>;
}
