namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Contracts
{
    internal interface IRtmpCommandHanlderMap
    {
        Type? GetHandlerType(string command);
    }
}