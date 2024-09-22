using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingClientNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpClientConnectionEventDispatcher
    {
        ValueTask RtmpHandshakeCompleteAsync(IRtmpSessionContext context);
        ValueTask RtmpConnectedAsync(IRtmpSessionContext context, IDictionary<string, object> commandObject, object? parameters);
        ValueTask RtmpConnectionRejectedAsync(IRtmpSessionContext context, IDictionary<string, object> commandObject, object? parameters);
    }

    internal interface IRtmpClientStreamEventDispatcher
    {
        ValueTask RtmpStreamCreated(IRtmpSessionContext context, uint streamId);
    }
}
