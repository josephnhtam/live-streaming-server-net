using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using Mediator;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEvents
{
    internal class RtmpChunkEvent : IRequest<RtmpEventConsumingResult>
    {
        public IRtmpClientSessionContext ClientContext { get; set; } = default!;
        public INetworkStreamReader NetworkStream { get; set; } = default!;
    }
}
