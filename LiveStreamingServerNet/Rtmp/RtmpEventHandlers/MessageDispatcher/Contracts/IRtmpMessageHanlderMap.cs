namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts
{
    public interface IRtmpMessageHanlderMap
    {
        Type? GetHandlerType(byte messageTypeId);
    }
}