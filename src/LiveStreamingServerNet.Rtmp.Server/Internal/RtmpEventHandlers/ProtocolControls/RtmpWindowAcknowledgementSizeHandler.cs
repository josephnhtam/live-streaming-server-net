using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.WindowAcknowledgementSize)]
    internal class RtmpWindowAcknowledgementSizeHandler : IRtmpMessageHandler<IRtmpClientSessionContext>
    {
        private readonly ILogger _logger;

        public RtmpWindowAcknowledgementSizeHandler(ILogger<RtmpWindowAcknowledgementSizeHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            clientContext.InWindowAcknowledgementSize = payloadBuffer.ReadUInt32BigEndian();
            _logger.WindowAcknowledgementSize(clientContext.Client.Id, clientContext.InWindowAcknowledgementSize);
            return ValueTask.FromResult(true);
        }
    }
}
