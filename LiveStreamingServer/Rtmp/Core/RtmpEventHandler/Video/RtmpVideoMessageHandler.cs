using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Video
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    public class RtmpVideoMessageHandler : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public RtmpVideoMessageHandler(ILogger<RtmpVideoMessageHandler> logger)
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
