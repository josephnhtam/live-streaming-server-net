using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers
{
    internal class RtmpChunkEventHandler : IRequestHandler<RtmpChunkEvent, bool>
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly IRtmpMessageDispatcher _dispatcher;
        private readonly IRtmpProtocolControlMessageSenderService _protocolControlMessageSender;
        private readonly ILogger _logger;

        public RtmpChunkEventHandler(
            INetBufferPool netBufferPool,
            IRtmpMessageDispatcher dispatcher,
            IRtmpProtocolControlMessageSenderService protocolControlMessageSender,
            ILogger<RtmpChunkEventHandler> logger)
        {
            _netBufferPool = netBufferPool;
            _dispatcher = dispatcher;
            _protocolControlMessageSender = protocolControlMessageSender;
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpChunkEvent @event, CancellationToken cancellationToken)
        {
            using var netBuffer = _netBufferPool.Obtain();

            if (await HandleChunkEvent(@event, netBuffer, cancellationToken))
            {
                HandleAcknowlegement(@event, netBuffer.Size);
                return true;
            }

            _logger.FailedToHandleChunkEvent(@event.ClientContext.Client.ClientId);

            return false;
        }

        private async Task<bool> HandleChunkEvent(RtmpChunkEvent @event, INetBuffer netBuffer, CancellationToken cancellationToken)
        {
            var basicHeader = await RtmpChunkBasicHeader.ReadAsync(netBuffer, @event.NetworkStream, cancellationToken);

            var chunkStreamContext = @event.ClientContext.GetChunkStreamContext(basicHeader.ChunkStreamId);

            var success = basicHeader.ChunkType switch
            {
                0 => await HandleChunkMessageHeaderType0Async(chunkStreamContext, @event, netBuffer, cancellationToken),
                1 => await HandleChunkMessageHeaderType1Async(chunkStreamContext, @event, netBuffer, cancellationToken),
                2 => await HandleChunkMessageHeaderType2Async(chunkStreamContext, @event, netBuffer, cancellationToken),
                3 => await HandleChunkMessageHeaderType3Async(chunkStreamContext, @event, netBuffer, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(basicHeader.ChunkType))
            };

            success &= await HandleChunkEventPayloadAsync(chunkStreamContext, @event, netBuffer, cancellationToken);
            return success;
        }

        private void HandleAcknowlegement(RtmpChunkEvent @event, int bufferSize)
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

        private async Task<bool> HandleChunkMessageHeaderType0Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            INetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType0.ReadAsync(netBuffer, @event.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 0;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;
            chunkStreamContext.MessageHeader.MessageStreamId = messageHeader.MessageStreamId;

            chunkStreamContext.MessageHeader.TimestampDelta = 0;
            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(netBuffer, @event.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.Timestamp = extendedTimestampHeader.extendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.Timestamp = messageHeader.Timestamp;
            }

            return true;
        }

        private async Task<bool> HandleChunkMessageHeaderType1Async(IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            INetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType1.ReadAsync(netBuffer, @event.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 1;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;

            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(netBuffer, @event.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.TimestampDelta = extendedTimestampHeader.extendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.TimestampDelta = messageHeader.TimestampDelta;
            }
            chunkStreamContext.MessageHeader.Timestamp += chunkStreamContext.MessageHeader.TimestampDelta;

            return true;
        }

        private async Task<bool> HandleChunkMessageHeaderType2Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            INetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType2.ReadAsync(netBuffer, @event.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 2;

            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(netBuffer, @event.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.TimestampDelta = extendedTimestampHeader.extendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.TimestampDelta = messageHeader.TimestampDelta;
            }
            chunkStreamContext.MessageHeader.Timestamp += chunkStreamContext.MessageHeader.TimestampDelta;

            return true;
        }

        private async Task<bool> HandleChunkMessageHeaderType3Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            INetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            chunkStreamContext.ChunkType = 3;

            var timestampDelta = chunkStreamContext.MessageHeader.HasExtendedTimestamp ?
                (await RtmpChunkExtendedTimestampHeader.ReadAsync(netBuffer, @event.NetworkStream, cancellationToken)).extendedTimestamp :
                chunkStreamContext.MessageHeader.TimestampDelta;

            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.MessageHeader.TimestampDelta = timestampDelta;
                chunkStreamContext.MessageHeader.Timestamp += timestampDelta;
            }

            return true;
        }

        private async Task<bool> HandleChunkEventPayloadAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkEvent @event, INetBuffer netBuffer, CancellationToken cancellationToken)
        {
            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.PayloadBuffer = _netBufferPool.Obtain();
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

                await netBuffer.CopyStreamData(@event.NetworkStream, chunkedPayloadLength, cancellationToken);
                netBuffer.CopyAllTo(payloadBuffer);
            }

            if (payloadBuffer.Size == messageLength)
            {
                payloadBuffer.Position = 0;
                return await DoHandleChunkEventPayloadAsync(chunkStreamContext, @event.ClientContext, cancellationToken);
            }

            return true;
        }

        private async Task<bool> DoHandleChunkEventPayloadAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, CancellationToken cancellationToken)
        {
            try
            {
                using var payloadBuffer = chunkStreamContext.PayloadBuffer ?? throw new InvalidOperationException();
                return await _dispatcher.DispatchAsync(chunkStreamContext, clientContext, cancellationToken);
            }
            finally
            {
                chunkStreamContext.PayloadBuffer = null;
            }
        }
    }
}
