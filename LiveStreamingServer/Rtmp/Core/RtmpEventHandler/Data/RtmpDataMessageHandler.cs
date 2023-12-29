using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Data
{
    [RtmpMessageType(RtmpMessageType.DataMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.DataMessageAmf3)]
    public class RtmpDataMessageHandler : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public RtmpDataMessageHandler(ILogger<RtmpDataMessageHandler> logger)
        {
            _logger = logger;
        }

        public Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent message,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
