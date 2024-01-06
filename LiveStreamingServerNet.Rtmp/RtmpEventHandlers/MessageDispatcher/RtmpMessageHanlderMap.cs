using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher
{
    internal class RtmpMessageHanlderMap : IRtmpMessageHanlderMap
    {
        private readonly IDictionary<byte, Type> _handlerMap;

        public RtmpMessageHanlderMap(IDictionary<byte, Type> handlerMap)
        {
            _handlerMap = handlerMap;
        }

        public Type? GetHandlerType(byte messageTypeId)
        {
            return _handlerMap.TryGetValue(messageTypeId, out var handlerType) ? handlerType : null;
        }
    }
}
