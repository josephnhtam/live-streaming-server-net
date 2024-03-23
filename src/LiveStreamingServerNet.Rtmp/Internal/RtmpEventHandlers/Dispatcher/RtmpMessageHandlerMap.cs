using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher
{
    internal class RtmpMessageHandlerMap : IRtmpMessageHandlerMap
    {
        private readonly IDictionary<byte, Type> _handlerMap;

        public RtmpMessageHandlerMap(IDictionary<byte, Type> handlerMap)
        {
            _handlerMap = handlerMap;
        }

        public Type? GetHandlerType(byte messageTypeId)
        {
            return _handlerMap.TryGetValue(messageTypeId, out var handlerType) ? handlerType : null;
        }
    }
}
