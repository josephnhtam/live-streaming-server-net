using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.Acknowledgement)]
    public class RtmpAcknowledgementHandler : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public RtmpAcknowledgementHandler(ILogger<RtmpAcknowledgementHandler> logger)
        {
            _logger = logger;
        }

        public Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            _logger.AcknowledgementReceived(peerContext.Peer.PeerId);
            return Task.FromResult(true);
        }
    }
}
