using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher
{
    internal class RtmpMessageDispatcher<TContext> : IRtmpMessageDispatcher<TContext>
    {
        private readonly IServiceProvider _services;
        private readonly IReadOnlyDictionary<byte, IRtmpMessageHandler<TContext>> _handlerCache;

        public RtmpMessageDispatcher(IServiceProvider services, IRtmpMessageHandlerMap handlerMap)
        {
            _services = services;
            _handlerCache = CreateHandlerCache(handlerMap);
        }

        public async ValueTask<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, TContext context, CancellationToken cancellationToken)
        {
            var handler = _handlerCache.GetValueOrDefault(chunkStreamContext.MessageHeader.MessageTypeId) ??
                throw new InvalidOperationException($"No handler found for message type {chunkStreamContext.MessageHeader.MessageTypeId}");

            return await handler.HandleAsync(chunkStreamContext, context, chunkStreamContext.PayloadBuffer!, cancellationToken);
        }

        private IReadOnlyDictionary<byte, IRtmpMessageHandler<TContext>> CreateHandlerCache(IRtmpMessageHandlerMap handlerMap)
        {
            var handlerCache = new Dictionary<byte, IRtmpMessageHandler<TContext>>();

            foreach (var (messageTypeId, handlerType) in handlerMap.GetHandlers())
            {
                handlerCache[messageTypeId] = _services.GetRequiredService(handlerType) as IRtmpMessageHandler<TContext> ??
                    throw new InvalidOperationException($"No handler found for message type {messageTypeId}");
            }

            return handlerCache;
        }
    }
}
