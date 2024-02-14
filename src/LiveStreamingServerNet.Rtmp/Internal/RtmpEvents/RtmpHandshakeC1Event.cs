using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using MediatR;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEvents
{
    internal record struct RtmpHandshakeC1Event(
        IRtmpClientContext ClientContext,
        ReadOnlyStream NetworkStream) : IRequest<RtmpEventConsumingResult>;
}
