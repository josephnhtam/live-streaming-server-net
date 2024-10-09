using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    internal class RtmpVideoMessageHandler : IRtmpMessageHandler<IRtmpClientSessionContext>
    {
        private readonly IRtmpVideoDataProcessorService _videoDataProcessor;
        private readonly ILogger _logger;

        public RtmpVideoMessageHandler(
            IRtmpVideoDataProcessorService videoDataProcessor,
            ILogger<RtmpVideoMessageHandler> logger)
        {
            _videoDataProcessor = videoDataProcessor;
            _logger = logger;
        }

        public async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var timestamp = chunkStreamContext.Timestamp;
            var publishStreamContext = clientContext.GetStreamContext(streamId)?.PublishContext;

            if (publishStreamContext == null)
            {
                _logger.PublishStreamNotYetCreated(clientContext.Client.Id, streamId);
                return false;
            }

            return await _videoDataProcessor.ProcessVideoDataAsync(publishStreamContext, timestamp, payloadBuffer);
        }
    }
}
