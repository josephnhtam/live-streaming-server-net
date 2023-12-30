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
            IRtmpClientPeerContext peerContext,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            peerContext.InChunkSize = payloadBuffer.ReadUInt32BigEndian();
            _logger.LogDebug("PeerId: {PeerId} | SetChunkSize: {InChunkSize}", peerContext.Peer.PeerId, peerContext.InChunkSize);
            return Task.FromResult(true);
        }
    }
}
