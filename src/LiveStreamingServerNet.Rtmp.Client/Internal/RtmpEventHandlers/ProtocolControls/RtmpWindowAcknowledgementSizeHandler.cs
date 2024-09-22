using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.WindowAcknowledgementSize)]
    internal class RtmpWindowAcknowledgementSizeHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        private readonly ILogger _logger;

        public RtmpWindowAcknowledgementSizeHandler(ILogger<RtmpWindowAcknowledgementSizeHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            context.InWindowAcknowledgementSize = payloadBuffer.ReadUInt32BigEndian();
            _logger.WindowAcknowledgementSize(context.Session.Id, context.InWindowAcknowledgementSize);
            return ValueTask.FromResult(true);
        }
    }
}
