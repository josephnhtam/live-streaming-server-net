using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents
{
    internal record RtmpHandshakeInitiationEvent(IRtmpSessionContext Context) : IRequest<bool>;
}