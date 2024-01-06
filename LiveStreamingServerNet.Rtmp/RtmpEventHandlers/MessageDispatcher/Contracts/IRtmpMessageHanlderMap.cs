namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts
{
    internal interface IRtmpMessageHanlderMap
    {
        Type? GetHandlerType(byte messageTypeId);
    }
}