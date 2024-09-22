using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingClientNet.Rtmp.Client.Contracts
{
    public interface IRtmpClientConnectionEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpHandshakeCompleteAsync(IEventContext context);
        ValueTask RtmpConnectedAsync(IEventContext context, IDictionary<string, object> commandObject, object? parameters);
        ValueTask RtmpConnectionRejectedAsync(IEventContext context, IDictionary<string, object> commandObject, object? parameters);
    }

    public interface IRtmpClientStreamEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpStreamCreated(IEventContext context, uint streamId);
    }
}
