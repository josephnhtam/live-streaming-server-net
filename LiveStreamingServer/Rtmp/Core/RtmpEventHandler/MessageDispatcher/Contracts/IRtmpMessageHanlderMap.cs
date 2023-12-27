namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts
{
    public interface IRtmpMessageHanlderMap
    {
        Type? GetHandlerType(byte messageTypeId);
    }
}