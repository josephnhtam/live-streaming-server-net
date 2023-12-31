using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.WindowAcknowledgementSize)]
    public class WindowAcknowledgementSize : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public WindowAcknowledgementSize(ILogger<WindowAcknowledgementSize> logger)
        {
            _logger = logger;
        }

        public Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            peerContext.InWindowAcknowledgementSize = payloadBuffer.ReadUInt32BigEndian();
            _logger.LogDebug("PeerId: {PeerId} | WindowAcknowledgementSize: {InWindowAcknowledgementSize}", peerContext.Peer.PeerId, peerContext.InWindowAcknowledgementSize);
            return Task.FromResult(true);
        }
    }
}
