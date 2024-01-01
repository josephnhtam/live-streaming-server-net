namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Contracts
{
    public interface IRtmpCommandHanlderMap
    {
        Type? GetHandlerType(string command);
    }
}