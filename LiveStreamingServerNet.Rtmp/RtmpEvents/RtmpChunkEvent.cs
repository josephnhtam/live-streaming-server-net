using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Rtmp.Contracts;
using MediatR;

namespace LiveStreamingServerNet.Rtmp.RtmpEvents
{
    internal record struct RtmpChunkEvent(
        IRtmpClientPeerContext PeerContext,
        ReadOnlyNetworkStream NetworkStream) : IRequest<bool>;
}
