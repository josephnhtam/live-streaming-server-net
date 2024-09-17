using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers
{
    internal class RtmpChunkEventHandler : IRequestHandler<RtmpChunkEvent, RtmpEventConsumingResult>
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IRtmpMessageDispatcher _dispatcher;
        private readonly IRtmpProtocolControlMessageSenderService _protocolControlMessageSender;
        private readonly ILogger _logger;

        public RtmpChunkEventHandler(
            IDataBufferPool dataBufferPool,
            IRtmpMessageDispatcher dispatcher,
            IRtmpProtocolControlMessageSenderService protocolControlMessageSender,
            ILogger<RtmpChunkEventHandler> logger)
        {
            _dataBufferPool = dataBufferPool;
            _dispatcher = dispatcher;
            _protocolControlMessageSender = protocolControlMessageSender;
            _logger = logger;
        }

        public async ValueTask<RtmpEventConsumingResult> Handle(RtmpChunkEvent @event, CancellationToken cancellationToken)
        {
            var result = await HandleChunkEvent(@event, cancellationToken);

            if (result.Succeeded)
            {
                HandleAcknowledgement(@event, result.ConsumedBytes);
                return result;
            }

            _logger.FailedToHandleChunkEvent(@event.ClientContext.Client.ClientId);
            return result;
        }

        private async ValueTask<RtmpEventConsumingResult> HandleChunkEvent(RtmpChunkEvent @event, CancellationToken cancellationToken)
        {
            var headerBuffer = _dataBufferPool.Obtain();

            try
            {
                var basicHeader = await RtmpChunkBasicHeader.ReadAsync(headerBuffer, @event.NetworkStream, cancellationToken);

                var chunkStreamContext = @event.ClientContext.GetChunkStreamContext(basicHeader.ChunkStreamId);

                var success = basicHeader.ChunkType switch
                {
                    0 => await HandleChunkMessageHeaderType0Async(chunkStreamContext, @event, headerBuffer, cancellationToken),
                    1 => await HandleChunkMessageHeaderType1Async(chunkStreamContext, @event, headerBuffer, cancellationToken),
                    2 => await HandleChunkMessageHeaderType2Async(chunkStreamContext, @event, headerBuffer, cancellationToken),
                    3 => await HandleChunkMessageHeaderType3Async(chunkStreamContext, @event, headerBuffer, cancellationToken),
                    _ => throw new ArgumentOutOfRangeException(nameof(basicHeader.ChunkType))
                };

                if (!success)
                    return new RtmpEventConsumingResult(false, 0);

                return await HandleChunkEventPayloadAsync(chunkStreamContext, @event, headerBuffer.Size, cancellationToken);
            }
            finally
            {
                _dataBufferPool.Recycle(headerBuffer);
            }
        }

        private void HandleAcknowledgement(RtmpChunkEvent @event, int consumedBytes)
        {
            var clientContext = @event.ClientContext;

            if (clientContext.OutWindowAcknowledgementSize == 0)
                return;

            clientContext.SequenceNumber += (uint)consumedBytes;
            if (clientContext.SequenceNumber - clientContext.LastAcknowledgedSequenceNumber >= clientContext.OutWindowAcknowledgementSize)
            {
                _protocolControlMessageSender.Acknowledgement(clientContext, clientContext.SequenceNumber);

                const uint overflow = 0xf0000000;
                if (clientContext.SequenceNumber >= overflow)
                {
                    clientContext.SequenceNumber -= overflow;
                    clientContext.LastAcknowledgedSequenceNumber -= overflow;
                }

                clientContext.LastAcknowledgedSequenceNumber = clientContext.SequenceNumber;
            }
        }

        private async ValueTask<bool> HandleChunkMessageHeaderType0Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType0.ReadAsync(dataBuffer, @event.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 0;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;
            chunkStreamContext.MessageHeader.MessageStreamId = messageHeader.MessageStreamId;

            chunkStreamContext.MessageHeader.TimestampDelta = 0;
            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, @event.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.Timestamp = extendedTimestampHeader.ExtendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.Timestamp = messageHeader.Timestamp;
            }

            return true;
        }

        private async ValueTask<bool> HandleChunkMessageHeaderType1Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType1.ReadAsync(dataBuffer, @event.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 1;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;

            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, @event.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.TimestampDelta = extendedTimestampHeader.ExtendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.TimestampDelta = messageHeader.TimestampDelta;
            }
            chunkStreamContext.MessageHeader.Timestamp += chunkStreamContext.MessageHeader.TimestampDelta;

            return true;
        }

        private async ValueTask<bool> HandleChunkMessageHeaderType2Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType2.ReadAsync(dataBuffer, @event.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 2;

            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, @event.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.TimestampDelta = extendedTimestampHeader.ExtendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.TimestampDelta = messageHeader.TimestampDelta;
            }
            chunkStreamContext.MessageHeader.Timestamp += chunkStreamContext.MessageHeader.TimestampDelta;

            return true;
        }

        private async ValueTask<bool> HandleChunkMessageHeaderType3Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            chunkStreamContext.ChunkType = 3;

            var timestampDelta = chunkStreamContext.MessageHeader.HasExtendedTimestamp ?
                (await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, @event.NetworkStream, cancellationToken)).ExtendedTimestamp :
                chunkStreamContext.MessageHeader.TimestampDelta;

            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.MessageHeader.TimestampDelta = timestampDelta;
                chunkStreamContext.MessageHeader.Timestamp += timestampDelta;
            }

            return true;
        }

        private async ValueTask<RtmpEventConsumingResult> HandleChunkEventPayloadAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            int headerSize,
            CancellationToken cancellationToken)
        {
            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.PayloadBuffer = _dataBufferPool.Obtain();
            }

            var clientContext = @event.ClientContext;
            var payloadBuffer = chunkStreamContext.PayloadBuffer!;
            var messageLength = chunkStreamContext.MessageHeader.MessageLength;
            var chunkedPayloadLength = 0;

            if (payloadBuffer.Size < messageLength)
            {
                chunkedPayloadLength = (int)Math.Min(
                    messageLength - payloadBuffer.Size,
                    clientContext.InChunkSize - payloadBuffer.Size % clientContext.InChunkSize
                );

                await payloadBuffer.AppendStreamData(@event.NetworkStream, chunkedPayloadLength, cancellationToken);
            }

            if (payloadBuffer.Size == messageLength)
            {
                payloadBuffer.Position = 0;
                var succeeded = await DispatchRtmpMessageAsync(chunkStreamContext, @event.ClientContext, cancellationToken);
                return new RtmpEventConsumingResult(succeeded, headerSize + chunkedPayloadLength);
            }

            return new RtmpEventConsumingResult(true, headerSize + chunkedPayloadLength);
        }

        private async ValueTask<bool> DispatchRtmpMessageAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, CancellationToken cancellationToken)
        {
            Debug.Assert(chunkStreamContext.PayloadBuffer != null);

            try
            {
                return await _dispatcher.DispatchAsync(chunkStreamContext, clientContext, cancellationToken);
            }
            finally
            {
                _dataBufferPool.Recycle(chunkStreamContext.PayloadBuffer);
                chunkStreamContext.PayloadBuffer = null;
            }
        }
    }
}
