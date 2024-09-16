using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    [RtmpMessageType(RtmpMessageType.CommandMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.CommandMessageAmf3)]
    internal class RtmpCommandMessageHandler : IRtmpMessageHandler<IRtmpClientSessionContext>
    {
        private readonly IRtmpCommandDispatcher<IRtmpClientSessionContext> _dispatcher;

        public RtmpCommandMessageHandler(IRtmpCommandDispatcher<IRtmpClientSessionContext> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientSessionContext clientContext, IDataBuffer payloadBuffer, CancellationToken cancellationToken)
        {
            return await _dispatcher.DispatchAsync(chunkStreamContext, clientContext, payloadBuffer, cancellationToken);
        }
    }
}
