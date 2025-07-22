using LiveStreamingServerNet.Rtmp.Client.Configurations;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.SetChunkSize)]
    internal class RtmpSetChunkSizeHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        private readonly RtmpClientConfiguration _config;
        private readonly ILogger _logger;

        public RtmpSetChunkSizeHandler(IOptions<RtmpClientConfiguration> config, ILogger<RtmpSetChunkSizeHandler> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var inChunkSize = payloadBuffer.ReadUInt32BigEndian();

            if (_config.MaxInChunkSize > 0 && inChunkSize > _config.MaxInChunkSize)
            {
                _logger.MaxInChunkSizeExceeded(context.Session.Id, inChunkSize, _config.MaxInChunkSize);
                return ValueTask.FromResult(false);
            }

            context.InChunkSize = inChunkSize;
            _logger.SetChunkSize(context.Session.Id, context.InChunkSize);
            return ValueTask.FromResult(true);
        }
    }
}
