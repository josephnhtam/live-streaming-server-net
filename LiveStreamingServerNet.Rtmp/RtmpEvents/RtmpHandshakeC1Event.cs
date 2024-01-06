using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Rtmp.Contracts;
using MediatR;

namespace LiveStreamingServerNet.Rtmp.RtmpEvents
{
    internal record struct RtmpHandshakeC1Event(
        IRtmpClientPeerContext PeerContext,
        ReadOnlyNetworkStream NetworkStream) : IRequest<bool>;
}
