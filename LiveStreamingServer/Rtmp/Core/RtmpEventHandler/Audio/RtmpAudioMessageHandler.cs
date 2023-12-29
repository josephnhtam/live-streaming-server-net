using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Audio
{
    [RtmpMessageType(RtmpMessageType.AudioMessage)]
    public class RtmpAudioMessageHandler : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public RtmpAudioMessageHandler(ILogger<RtmpAudioMessageHandler> logger)
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
