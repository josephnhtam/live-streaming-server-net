using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher
{
    internal class RtmpCommandHandlerMap : IRtmpCommandHanlderMap
    {
        private readonly IReadOnlyDictionary<string, Type> _handlerMap;

        public RtmpCommandHandlerMap(IReadOnlyDictionary<string, Type> handlerMap)
        {
            _handlerMap = handlerMap;
        }

        public Type? GetHandlerType(string command)
        {
            return _handlerMap.TryGetValue(command, out var handlerType) ? handlerType : null;
        }
    }
}
