using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.ProtocolControls
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
