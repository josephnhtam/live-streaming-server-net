namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Contracts
{
    public interface IRtmpMessageHanlderMap
    {
        Type? GetHandlerType(byte messageTypeId);
    }
}