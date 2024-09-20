using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using Mediator;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents
{
    internal record RtmpHandshakeInitiationEvent(IRtmpSessionContext Context) : IRequest<bool>;
}