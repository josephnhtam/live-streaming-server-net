using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.SetChunkSize)]
    internal class RtmpSetChunkSizeHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        private readonly ILogger _logger;

        public RtmpSetChunkSizeHandler(ILogger<RtmpSetChunkSizeHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            context.InChunkSize = payloadBuffer.ReadUInt32BigEndian();
            _logger.SetChunkSize(context.Session.Id, context.InChunkSize);
            return ValueTask.FromResult(true);
        }
    }
}
