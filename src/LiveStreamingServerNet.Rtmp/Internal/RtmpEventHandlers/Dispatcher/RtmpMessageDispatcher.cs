using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher
{
    internal class RtmpMessageDispatcher : IRtmpMessageDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly IReadOnlyDictionary<byte, IRtmpMessageHandler> _handlerCache;

        public RtmpMessageDispatcher(IServiceProvider services, IRtmpMessageHandlerMap handlerMap)
        {
            _services = services;
            _handlerCache = CreateHandlerCache(handlerMap);
        }

        public async ValueTask<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, CancellationToken cancellationToken)
        {
            var handler = _handlerCache.GetValueOrDefault(chunkStreamContext.MessageHeader.MessageTypeId) ??
                throw new InvalidOperationException($"No handler found for message type {chunkStreamContext.MessageHeader.MessageTypeId}");

            return await handler.HandleAsync(chunkStreamContext, clientContext, chunkStreamContext.PayloadBuffer!, cancellationToken);
        }

        private IReadOnlyDictionary<byte, IRtmpMessageHandler> CreateHandlerCache(IRtmpMessageHandlerMap handlerMap)
        {
            var handlerCache = new Dictionary<byte, IRtmpMessageHandler>();

            foreach (var (messageTypeId, handlerType) in handlerMap.GetHandlers())
            {
                handlerCache[messageTypeId] = _services.GetRequiredService(handlerType) as IRtmpMessageHandler ??
                    throw new InvalidOperationException($"No handler found for message type {messageTypeId}");
            }

            return handlerCache;
        }
    }
}
