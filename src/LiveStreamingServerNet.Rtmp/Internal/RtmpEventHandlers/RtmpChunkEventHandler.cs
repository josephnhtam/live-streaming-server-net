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
            var dataBuffer = _dataBufferPool.Obtain();

            try
            {
                var result = await HandleChunkEvent(@event, dataBuffer, cancellationToken);
                if (result.Succeeded)
                {
                    HandleAcknowledgement(@event, dataBuffer.Size);
                    return result;
                }

                _logger.FailedToHandleChunkEvent(@event.ClientContext.Client.ClientId);

                return result;
            }
            finally
            {
                _dataBufferPool.Recycle(dataBuffer);
            }
        }

        private async ValueTask<RtmpEventConsumingResult> HandleChunkEvent(RtmpChunkEvent @event, IDataBuffer dataBuffer, CancellationToken cancellationToken)
        {
            var basicHeader = await RtmpChunkBasicHeader.ReadAsync(dataBuffer, @event.NetworkStream, cancellationToken);

            var chunkStreamContext = @event.ClientContext.GetChunkStreamContext(basicHeader.ChunkStreamId);

            var success = basicHeader.ChunkType switch
            {
                0 => await HandleChunkMessageHeaderType0Async(chunkStreamContext, @event, dataBuffer, cancellationToken),
                1 => await HandleChunkMessageHeaderType1Async(chunkStreamContext, @event, dataBuffer, cancellationToken),
                2 => await HandleChunkMessageHeaderType2Async(chunkStreamContext, @event, dataBuffer, cancellationToken),
                3 => await HandleChunkMessageHeaderType3Async(chunkStreamContext, @event, dataBuffer, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(basicHeader.ChunkType))
            };

            success &= await HandleChunkEventPayloadAsync(chunkStreamContext, @event, cancellationToken);

            return new RtmpEventConsumingResult(success, (int)@event.ClientContext.InChunkSize);
        }

        private void HandleAcknowledgement(RtmpChunkEvent @event, int bufferSize)
        {
            var clientContext = @event.ClientContext;

            if (clientContext.OutWindowAcknowledgementSize == 0)
                return;

            clientContext.SequenceNumber += (uint)bufferSize;
            if (clientContext.SequenceNumber - clientContext.LastAcknowledgedSequenceNumber >= clientContext.OutWindowAcknowledgementSize)
            {
                clientContext.LastAcknowledgedSequenceNumber = clientContext.SequenceNumber;
                _protocolControlMessageSender.Acknowledgement(clientContext, clientContext.SequenceNumber);

                const uint overflow = 0xf0000000;
                if (clientContext.SequenceNumber >= overflow)
                {
                    clientContext.SequenceNumber -= overflow;
                    clientContext.LastAcknowledgedSequenceNumber -= overflow;
                }
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

        private async ValueTask<bool> HandleChunkEventPayloadAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkEvent @event, CancellationToken cancellationToken)
        {
            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.PayloadBuffer = _dataBufferPool.Obtain();
            }

            var clientContext = @event.ClientContext;
            var payloadBuffer = chunkStreamContext.PayloadBuffer!;
            int messageLength = chunkStreamContext.MessageHeader.MessageLength;

            if (payloadBuffer.Size < messageLength)
            {
                var chunkedPayloadLength = (int)Math.Min(
                    messageLength - payloadBuffer.Size,
                    clientContext.InChunkSize - payloadBuffer.Size % clientContext.InChunkSize
                );

                await payloadBuffer.AppendStreamData(@event.NetworkStream, chunkedPayloadLength, cancellationToken);
            }

            if (payloadBuffer.Size == messageLength)
            {
                payloadBuffer.Position = 0;
                return await DispatchRtmpMessageAsync(chunkStreamContext, @event.ClientContext, cancellationToken);
            }

            return true;
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
