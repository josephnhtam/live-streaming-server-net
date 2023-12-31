using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.ProtocolControls
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
            //_logger.LogDebug("PeerId: {PeerId} | Acknowledgement", peerContext.Peer.PeerId);
            return Task.FromResult(true);
        }
    }
}
