namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts
{
    internal interface IRtmpCommandHanlderMap
    {
        Type? GetHandlerType(string command);
    }
}