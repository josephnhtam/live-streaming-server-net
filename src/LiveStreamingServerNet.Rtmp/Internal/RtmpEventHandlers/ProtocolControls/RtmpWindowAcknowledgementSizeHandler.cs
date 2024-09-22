using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.WindowAcknowledgementSize)]
    internal class RtmpWindowAcknowledgementSizeHandler : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public RtmpWindowAcknowledgementSizeHandler(ILogger<RtmpWindowAcknowledgementSizeHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            clientContext.InWindowAcknowledgementSize = payloadBuffer.ReadUInt32BigEndian();
            _logger.WindowAcknowledgementSize(clientContext.Client.ClientId, clientContext.InWindowAcknowledgementSize);
            return ValueTask.FromResult(true);
        }
    }
}
