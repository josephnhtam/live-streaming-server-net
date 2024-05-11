using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher
{
    internal class RtmpMessageHandlerMap : IRtmpMessageHandlerMap
    {
        private readonly IReadOnlyDictionary<byte, Type> _handlerMap;

        public RtmpMessageHandlerMap(IReadOnlyDictionary<byte, Type> handlerMap)
        {
            _handlerMap = handlerMap;
        }

        public Type? GetHandlerType(byte messageTypeId)
        {
            return _handlerMap.TryGetValue(messageTypeId, out var handlerType) ? handlerType : null;
        }

        public IReadOnlyDictionary<byte, Type> GetHandlers()
        {
            return _handlerMap;
        }
    }
}
