using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.Commands
{
    [RtmpMessageType(RtmpMessageType.CommandMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.CommandMessageAmf3)]
    internal class RtmpCommandMessageHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        private readonly IRtmpCommandDispatcher<IRtmpSessionContext> _dispatcher;

        public RtmpCommandMessageHandler(IRtmpCommandDispatcher<IRtmpSessionContext> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            return await _dispatcher.DispatchAsync(chunkStreamContext, context, payloadBuffer, cancellationToken);
        }
    }
}
