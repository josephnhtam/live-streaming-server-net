using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.SetChunkSize)]
    internal class RtmpSetChunkSizeHandler : IRtmpMessageHandler<IRtmpClientSessionContext>
    {
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        public RtmpSetChunkSizeHandler(
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpSetChunkSizeHandler> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var inChunkSize = payloadBuffer.ReadUInt32BigEndian();

            if (_config.MaxInChunkSize > 0 && inChunkSize > _config.MaxInChunkSize)
            {
                _logger.MaxInChunkSizeExceeded(clientContext.Client.Id, inChunkSize, _config.MaxInChunkSize);
                return ValueTask.FromResult(false);
            }

            clientContext.InChunkSize = inChunkSize;
            _logger.SetChunkSize(clientContext.Client.Id, clientContext.InChunkSize);
            return ValueTask.FromResult(true);
        }
    }
}
