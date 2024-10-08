using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents
{
    internal record RtmpHandshakeC1Event(
        IRtmpClientSessionContext ClientContext,
        INetworkStreamReader NetworkStream) : IRequest<RtmpEventConsumingResult>;
}
