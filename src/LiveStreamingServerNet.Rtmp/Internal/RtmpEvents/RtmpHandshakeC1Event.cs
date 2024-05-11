using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using Mediator;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEvents
{
    internal record RtmpHandshakeC1Event(
        IRtmpClientContext ClientContext,
        INetworkStreamReader NetworkStream) : IRequest<RtmpEventConsumingResult>;
}
