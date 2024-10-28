using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher
{
    internal class RtmpMessageDispatcher<TContext> : IRtmpMessageDispatcher<TContext>
    {
        private readonly IReadOnlyDictionary<byte, IRtmpMessageHandler<TContext>> _handlerCache;
        private readonly ILogger _logger;

        public RtmpMessageDispatcher(IServiceProvider services, IRtmpMessageHandlerMap handlerMap, ILogger<RtmpMessageDispatcher<TContext>> logger)
        {
            _handlerCache = CreateHandlerCache(services, handlerMap);
            _logger = logger;
        }

        public async ValueTask<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, TContext context, CancellationToken cancellationToken)
        {
            var handler = _handlerCache.GetValueOrDefault(chunkStreamContext.MessageHeader.MessageTypeId);

            if (handler == null)
            {
                _logger.MessageHandlerNotFound(chunkStreamContext.ChunkStreamId, chunkStreamContext.MessageHeader.MessageTypeId);
                return true;
            }

            return await handler.HandleAsync(chunkStreamContext, context, chunkStreamContext.PayloadBuffer!, cancellationToken);
        }

        private IReadOnlyDictionary<byte, IRtmpMessageHandler<TContext>> CreateHandlerCache(
            IServiceProvider services, IRtmpMessageHandlerMap handlerMap)
        {
            var handlerCache = new Dictionary<byte, IRtmpMessageHandler<TContext>>();

            foreach (var (messageTypeId, handlerType) in handlerMap.GetHandlers())
            {
                handlerCache[messageTypeId] = services.GetRequiredService(handlerType) as IRtmpMessageHandler<TContext> ??
                    throw new InvalidOperationException($"No handler found for message type {messageTypeId}");
            }

            return handlerCache;
        }
    }
}
