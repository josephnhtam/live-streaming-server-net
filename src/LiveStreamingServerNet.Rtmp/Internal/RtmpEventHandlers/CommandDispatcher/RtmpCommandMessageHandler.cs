using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher
{
    [RtmpMessageType(RtmpMessageType.CommandMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.CommandMessageAmf3)]
    internal class RtmpCommandMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpCommandDispatcher _dispatcher;

        public RtmpCommandMessageHandler(IRtmpCommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, INetBuffer payloadBuffer, CancellationToken cancellationToken)
        {
            return await _dispatcher.DispatchAsync(chunkStreamContext, clientContext, payloadBuffer, cancellationToken);
        }
    }
}
