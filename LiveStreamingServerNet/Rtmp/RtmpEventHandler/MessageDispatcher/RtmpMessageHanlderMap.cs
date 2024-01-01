using LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher
{
    public class RtmpMessageHanlderMap : IRtmpMessageHanlderMap
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
