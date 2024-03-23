using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher
{
    internal class RtmpMessageDispatcher : IRtmpMessageDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpMessageHandlerMap _handlerMap;

        public RtmpMessageDispatcher(IServiceProvider services, IRtmpMessageHandlerMap handlerMap)
        {
            _services = services;
            _handlerMap = handlerMap;
        }

        public async ValueTask<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, CancellationToken cancellationToken)
        {
            var messageTypeId = chunkStreamContext.MessageHeader.MessageTypeId;
            var handlerType = _handlerMap.GetHandlerType(messageTypeId) ??
                throw new InvalidOperationException($"No handler found for message type {messageTypeId}");

            var handler = (_services.GetRequiredService(handlerType) as IRtmpMessageHandler)!;
            return await handler.HandleAsync(chunkStreamContext, clientContext, chunkStreamContext.PayloadBuffer!, cancellationToken);
        }
    }
}
