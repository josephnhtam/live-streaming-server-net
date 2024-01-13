namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Contracts
{
    internal interface IRtmpCommandHanlderMap
    {
        Type? GetHandlerType(string command);
    }
}