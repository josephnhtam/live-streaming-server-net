using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeC2EventHandler : IRequestHandler<RtmpHandshakeC2Event, RtmpEventConsumingResult>
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;

        private const int HandshakeC2Size = 1536;

        public RtmpHandshakeC2EventHandler(
            IDataBufferPool dataBufferPool,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            ILogger<RtmpHandshakeC2EventHandler> logger)
        {
            _dataBufferPool = dataBufferPool;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        // todo: add validation
        public async ValueTask<RtmpEventConsumingResult> Handle(RtmpHandshakeC2Event @event, CancellationToken cancellationToken)
        {
            var incomingBuffer = _dataBufferPool.Obtain();

            try
            {
                await incomingBuffer.FromStreamData(@event.NetworkStream, HandshakeC2Size, cancellationToken);

                @event.ClientContext.State = RtmpClientSessionState.HandshakeDone;

                _logger.HandshakeC2Handled(@event.ClientContext.Client.Id);

                await _eventDispatcher.RtmpClientHandshakeCompleteAsync(@event.ClientContext);

                return new RtmpEventConsumingResult(true, HandshakeC2Size);
            }
            finally
            {
                _dataBufferPool.Recycle(incomingBuffer);
            }
        }
    }
}
