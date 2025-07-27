using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers
{
    internal class RtmpHandshakeS1EventHandler : IRequestHandler<RtmpHandshakeS1Event, RtmpEventConsumingResult>
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly ILogger _logger;

        private const int HandshakeS1Size = 1536;

        public RtmpHandshakeS1EventHandler(IDataBufferPool dataBufferPool, ILogger<RtmpHandshakeS1EventHandler> logger)
        {
            _dataBufferPool = dataBufferPool;
            _logger = logger;
        }

        public async ValueTask<RtmpEventConsumingResult> Handle(RtmpHandshakeS1Event @event, CancellationToken cancellationToken)
        {
            var incomingBuffer = _dataBufferPool.Obtain();
            var outgoingBuffer = _dataBufferPool.Obtain();

            try
            {
                await incomingBuffer.FromStreamData(@event.NetworkStream, HandshakeS1Size, cancellationToken).ConfigureAwait(false);

                if (HandleHandshake(@event, incomingBuffer, outgoingBuffer))
                {
                    @event.Context.State = RtmpSessionState.HandshakeS2;
                    @event.Context.Session.Send(outgoingBuffer);

                    _logger.HandshakeS1Handled(@event.Context.Session.Id);

                    return new RtmpEventConsumingResult(true, HandshakeS1Size);
                }

                _logger.HandshakeS1HandlingFailed(@event.Context.Session.Id);

                return new RtmpEventConsumingResult(false, HandshakeS1Size);
            }
            finally
            {
                _dataBufferPool.Recycle(incomingBuffer);
                _dataBufferPool.Recycle(outgoingBuffer);
            }
        }

        private bool HandleHandshake(RtmpHandshakeS1Event @event, IDataBuffer incomingBuffer, IDataBuffer outgoingBuffer)
        {
            var context = @event.Context;

            var handshake = new SimpleHandshake();

            if (!handshake.ValidateS1(context, incomingBuffer))
                return false;

            handshake.WriteC2(context, incomingBuffer, outgoingBuffer);

            return true;
        }
    }
}