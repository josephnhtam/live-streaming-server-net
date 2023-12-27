using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.Headers;
using LiveStreamingServer.Rtmp.Core.RtmpMessages;
using LiveStreamingServer.Rtmp.Core.RtmpMessagetHandler.PayloadHandling.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler
{
    public class RtmpChunkMessageHandler : IRequestHandler<RtmpChunkMessage, bool>
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly IRtmpMessageMessagePayloadHandler _payloadHandler;
        private readonly ILogger _logger;

        public RtmpChunkMessageHandler(INetBufferPool netBufferPool, IRtmpMessageMessagePayloadHandler payloadHandler, ILogger<RtmpChunkMessageHandler> logger)
        {
            _netBufferPool = netBufferPool;
            _payloadHandler = payloadHandler;
            _logger = logger;
        }

        public async Task<bool> Handle(RtmpChunkMessage message, CancellationToken cancellationToken)
        {
            var netBuffer = _netBufferPool.ObtainNetBuffer();

            try
            {
                var basicHeader = await ChunkBasicHeader.ReadAsync(netBuffer, message.NetworkStream, cancellationToken);

                var chunkType = basicHeader.ChunkType;
                var chunkStreamId = basicHeader.ChunkStreamId;

                var chunkStreamContext = message.PeerContext.GetChunkStreamContext(chunkStreamId);

                var result = chunkType switch
                {
                    0 => await HandleChunkType0Async(chunkStreamContext, message, netBuffer, cancellationToken),
                    1 => await HandleChunkType1Async(chunkStreamContext, message, netBuffer, cancellationToken),
                    2 => await HandleChunkType2Async(chunkStreamContext, message, netBuffer, cancellationToken),
                    3 => await HandleChunkType3Async(chunkStreamContext, message, netBuffer, cancellationToken),
                    _ => throw new ArgumentOutOfRangeException(nameof(chunkType))
                };

                return await HandlePayloadAsync(chunkStreamContext, message, netBuffer, cancellationToken);
            }
            finally
            {
                _netBufferPool.RecycleNetBuffer(netBuffer);
            }
        }

        private async Task<bool> HandleChunkType0Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkMessage message,
            INetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await ChunkMessageHeaderType0.ReadAsync(netBuffer, message.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 0;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;
            chunkStreamContext.MessageHeader.MessageStreamId = messageHeader.MessageStreamId;

            chunkStreamContext.MessageHeader.TimestampDelta = 0;
            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.Timestamp == 0xffffff;
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await ChunkExtendedTimestampHeader.ReadAsync(netBuffer, message.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.Timestamp = extendedTimestampHeader.extendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.Timestamp = messageHeader.Timestamp;
            }

            return true;
        }

        private async Task<bool> HandleChunkType1Async(IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkMessage message,
            INetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await ChunkMessageHeaderType1.ReadAsync(netBuffer, message.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 1;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;

            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.TimestampDelta == 0xffffff;
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await ChunkExtendedTimestampHeader.ReadAsync(netBuffer, message.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.TimestampDelta = extendedTimestampHeader.extendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.TimestampDelta = messageHeader.TimestampDelta;
            }
            chunkStreamContext.MessageHeader.Timestamp += chunkStreamContext.MessageHeader.TimestampDelta;

            return true;
        }

        private async Task<bool> HandleChunkType2Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkMessage message,
            INetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await ChunkMessageHeaderType2.ReadAsync(netBuffer, message.NetworkStream, cancellationToken);

            chunkStreamContext.ChunkType = 2;

            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.TimestampDelta == 0xffffff;
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await ChunkExtendedTimestampHeader.ReadAsync(netBuffer, message.NetworkStream, cancellationToken);
                chunkStreamContext.MessageHeader.TimestampDelta = extendedTimestampHeader.extendedTimestamp;
            }
            else
            {
                chunkStreamContext.MessageHeader.TimestampDelta = messageHeader.TimestampDelta;
            }
            chunkStreamContext.MessageHeader.Timestamp += chunkStreamContext.MessageHeader.TimestampDelta;

            return true;
        }

        private async Task<bool> HandleChunkType3Async(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkMessage message,
            INetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            chunkStreamContext.ChunkType = 3;

            var timestampDelta = chunkStreamContext.MessageHeader.HasExtendedTimestamp ?
                (await ChunkExtendedTimestampHeader.ReadAsync(netBuffer, message.NetworkStream, cancellationToken)).extendedTimestamp :
                chunkStreamContext.MessageHeader.TimestampDelta;

            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.MessageHeader.TimestampDelta = timestampDelta;
                chunkStreamContext.MessageHeader.Timestamp += timestampDelta;
            }

            return true;
        }

        private async Task<bool> HandlePayloadAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkMessage message, INetBuffer netBuffer, CancellationToken cancellationToken)
        {
            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.PayloadBuffer = _netBufferPool.ObtainNetBuffer();
            }

            var peerContext = message.PeerContext;
            var payloadBuffer = chunkStreamContext.PayloadBuffer!;
            int messageLength = chunkStreamContext.MessageHeader.MessageLength;

            if (payloadBuffer.Size < messageLength)
            {
                var chunkedPayloadLength = Math.Min(
                    messageLength - payloadBuffer.Size,
                    peerContext.InChunkSize - (payloadBuffer.Size % peerContext.InChunkSize)
                );

                await netBuffer.CopyStreamData(message.NetworkStream, chunkedPayloadLength, cancellationToken);
                netBuffer.Flush(payloadBuffer);
            }

            if (payloadBuffer.Size == messageLength)
            {
                payloadBuffer.Position = 0;
                return await DoHandlePayloadAsync(chunkStreamContext, message, cancellationToken);
            }

            return true;
        }

        private async Task<bool> DoHandlePayloadAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkMessage message, CancellationToken cancellationToken)
        {
            var payloadBuffer = chunkStreamContext.PayloadBuffer ?? throw new InvalidOperationException();

            try
            {
                return await _payloadHandler.HandleAsync(chunkStreamContext, message, cancellationToken);
            }
            finally
            {
                chunkStreamContext.PayloadBuffer = null;
                _netBufferPool.RecycleNetBuffer(payloadBuffer);
            }
        }
    }
}
