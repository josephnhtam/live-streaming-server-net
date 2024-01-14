using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.Acknowledgement)]
    internal class RtmpAcknowledgementHandler : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public RtmpAcknowledgementHandler(ILogger<RtmpAcknowledgementHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            _logger.AcknowledgementReceived(clientContext.Client.ClientId);
            return ValueTask.FromResult(true);
        }
    }
}
