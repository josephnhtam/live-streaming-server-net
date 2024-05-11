namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts
{
    internal interface IRtmpMessageHandlerMap
    {
        Type? GetHandlerType(byte messageTypeId);
        IReadOnlyDictionary<byte, Type> GetHandlers();
    }
}