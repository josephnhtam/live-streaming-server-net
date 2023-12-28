using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.SetChunkSize)]
    public class RtmpSetChunkSizeHandler : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public RtmpSetChunkSizeHandler(ILogger<RtmpSetChunkSizeHandler> logger)
        {
            _logger = logger;
        }

        public Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent message,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            message.PeerContext.InChunkSize = payloadBuffer.ReadUInt32BigEndian();
            _logger.LogDebug("PeerId: {PeerId} | SetChunkSize: {InChunkSize}", message.PeerContext.Peer.PeerId, message.PeerContext.InChunkSize);
            return Task.FromResult(true);
        }
    }
}
