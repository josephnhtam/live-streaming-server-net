namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts
{
    internal interface IRtmpMessageHanlderMap
    {
        Type? GetHandlerType(byte messageTypeId);
    }
}