using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.AudioMessage)]
    internal class RtmpAudioMessageHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        private readonly ILogger _logger;

        public RtmpAudioMessageHandler(
            ILogger<RtmpAudioMessageHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var subscribeStreamContext = context.GetStreamContext(streamId)?.SubscribeContext;

            if (subscribeStreamContext == null)
            {
                _logger.SubscribeStreamNotYetCreated(context.Session.Id, streamId);
                return ValueTask.FromResult(true);
            }

            var rentedBuffer = payloadBuffer.ToRentedBuffer();

            try
            {
                subscribeStreamContext.ReceiveAudioData(new(rentedBuffer, chunkStreamContext.MessageHeader.Timestamp));

                return ValueTask.FromResult(true);
            }
            finally
            {
                rentedBuffer.Unclaim();
            }
        }
    }
}
