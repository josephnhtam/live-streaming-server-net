namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Contracts
{
    public interface IRtmpCommandHanlderMap
    {
        Type? GetHandlerType(string command);
    }
}