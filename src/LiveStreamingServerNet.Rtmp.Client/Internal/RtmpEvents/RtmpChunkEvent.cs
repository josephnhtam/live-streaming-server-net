using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents
{
    internal class RtmpChunkEvent : IRequest<RtmpEventConsumingResult>
    {
        public IRtmpSessionContext Context { get; set; } = default!;
        public INetworkStreamReader NetworkStream { get; set; } = default!;
    }
}