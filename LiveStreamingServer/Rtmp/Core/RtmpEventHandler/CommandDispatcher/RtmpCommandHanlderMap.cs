using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Contracts;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher
{
    public class RtmpCommandHanlderMap : IRtmpCommandHanlderMap
    {
        private readonly IDictionary<string, Type> _handlerMap;

        public RtmpCommandHanlderMap(IDictionary<string, Type> handlerMap)
        {
            _handlerMap = handlerMap;
        }

        public Type? GetHandlerType(string command)
        {
            return _handlerMap.TryGetValue(command, out var handlerType) ? handlerType : null;
        }
    }
}
