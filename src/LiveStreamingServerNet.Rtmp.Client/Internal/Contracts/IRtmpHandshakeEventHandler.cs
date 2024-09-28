using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpHandshakeEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpHandshakeCompleteAsync(IEventContext context);
    }
}
