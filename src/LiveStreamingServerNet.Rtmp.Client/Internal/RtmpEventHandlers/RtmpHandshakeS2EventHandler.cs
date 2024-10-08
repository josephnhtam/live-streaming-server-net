using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeS2EventHandler : IRequestHandler<RtmpHandshakeS2Event, RtmpEventConsumingResult>
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IRtmpHandshakeEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;

        private const int HandshakeS2Size = 1536;

        public RtmpHandshakeS2EventHandler(
            IDataBufferPool dataBufferPool,
            IRtmpHandshakeEventDispatcher eventDispatcher,
            ILogger<RtmpHandshakeS1EventHandler> logger)
        {
            _dataBufferPool = dataBufferPool;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async ValueTask<RtmpEventConsumingResult> Handle(RtmpHandshakeS2Event @event, CancellationToken cancellationToken)
        {
            var incomingBuffer = _dataBufferPool.Obtain();

            try
            {
                await incomingBuffer.FromStreamData(@event.NetworkStream, HandshakeS2Size, cancellationToken);

                @event.Context.State = RtmpSessionState.HandshakeDone;

                _logger.HandshakeS2Handled(@event.Context.Session.Id);

                await _eventDispatcher.RtmpHandshakeCompleteAsync(@event.Context);

                return new RtmpEventConsumingResult(true, HandshakeS2Size);
            }
            finally
            {
                _dataBufferPool.Recycle(incomingBuffer);
            }
        }
    }
}