using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher
{
    public class RtmpMessageDispatcher : IRtmpMessageDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpMessageHanlderMap _handlerMap;

        public RtmpMessageDispatcher(IServiceProvider services, IRtmpMessageHanlderMap handlerMap)
        {
            _services = services;
            _handlerMap = handlerMap;
        }

        public async Task<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, CancellationToken cancellationToken)
        {
            var messageTypeId = chunkStreamContext.MessageHeader.MessageTypeId;
            var handlerType = _handlerMap.GetHandlerType(messageTypeId) ??
                throw new InvalidOperationException($"No handler found for message type {messageTypeId}");

            var handler = (_services.GetRequiredService(handlerType) as IRtmpMessageHandler)!;
            return await handler.HandleAsync(chunkStreamContext, peerContext, chunkStreamContext.PayloadBuffer!, cancellationToken);
        }
    }
}
