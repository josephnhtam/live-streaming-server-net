namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Contracts
{
    public interface IRtmpCommandHanlderMap
    {
        Type? GetHandlerType(string command);
    }
}