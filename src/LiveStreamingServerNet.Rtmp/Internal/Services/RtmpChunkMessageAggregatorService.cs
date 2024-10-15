using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpChunkMessageAggregatorService : IRtmpChunkMessageAggregatorService
    {
        private readonly IDataBufferPool _dataBufferPool;

        public RtmpChunkMessageAggregatorService(IDataBufferPool dataBufferPool)
        {
            _dataBufferPool = dataBufferPool;
        }

        public async ValueTask<RtmpChunkMessageAggregationResult> AggregateChunkMessagesAsync(
            INetworkStreamReader networkStream,
            IRtmpChunkStreamContextProvider contextProvider,
            CancellationToken cancellationToken)
        {
            var headerBuffer = _dataBufferPool.Obtain();

            try
            {
                var basicHeader = await RtmpChunkBasicHeader.ReadAsync(headerBuffer, networkStream, cancellationToken);

                var chunkStreamContext = contextProvider.GetChunkStreamContext(basicHeader.ChunkStreamId);

                var messageHeaderSize = basicHeader.ChunkType switch
                {
                    0 => await HandleChunkMessageHeaderType0Async(chunkStreamContext, networkStream, headerBuffer, cancellationToken),
                    1 => await HandleChunkMessageHeaderType1Async(chunkStreamContext, networkStream, headerBuffer, cancellationToken),
                    2 => await HandleChunkMessageHeaderType2Async(chunkStreamContext, networkStream, headerBuffer, cancellationToken),
                    3 => await HandleChunkMessageHeaderType3Async(chunkStreamContext, networkStream, headerBuffer, cancellationToken),
                    _ => throw new ArgumentOutOfRangeException(nameof(basicHeader.ChunkType))
                };

                return await HandleChunkMessagePayloadAsync(
                    chunkStreamContext, networkStream, contextProvider.InChunkSize, basicHeader.Size + messageHeaderSize, cancellationToken);
            }
            finally
            {
                _dataBufferPool.Recycle(headerBuffer);
            }
        }

        private async ValueTask<int> HandleChunkMessageHeaderType0Async(
            IRtmpChunkStreamContext chunkStreamContext,
            INetworkStreamReader networkStream,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType0.ReadAsync(dataBuffer, networkStream, cancellationToken);
            var messageHeaderSize = messageHeader.Size;

            chunkStreamContext.ChunkType = 0;
            chunkStreamContext.MessageHeader.MessageStreamId = messageHeader.MessageStreamId;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.Timestamp = messageHeader.Timestamp;
            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();

            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, networkStream, cancellationToken);
                messageHeaderSize += extendedTimestampHeader.Size;

                chunkStreamContext.MessageHeader.Timestamp = extendedTimestampHeader.ExtendedTimestamp;
            }

            chunkStreamContext.Timestamp = chunkStreamContext.MessageHeader.Timestamp;

            return messageHeaderSize;
        }

        private async ValueTask<int> HandleChunkMessageHeaderType1Async(
            IRtmpChunkStreamContext chunkStreamContext,
            INetworkStreamReader networkStream,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType1.ReadAsync(dataBuffer, networkStream, cancellationToken);
            var messageHeaderSize = messageHeader.Size;

            chunkStreamContext.ChunkType = 1;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.Timestamp = messageHeader.TimestampDelta;
            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();

            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, networkStream, cancellationToken);
                messageHeaderSize += extendedTimestampHeader.Size;

                chunkStreamContext.MessageHeader.Timestamp = extendedTimestampHeader.ExtendedTimestamp;
            }

            chunkStreamContext.Timestamp += chunkStreamContext.MessageHeader.Timestamp;

            return messageHeaderSize;
        }

        private async ValueTask<int> HandleChunkMessageHeaderType2Async(
            IRtmpChunkStreamContext chunkStreamContext,
            INetworkStreamReader networkStream,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType2.ReadAsync(dataBuffer, networkStream, cancellationToken);
            var messageHeaderSize = messageHeader.Size;

            chunkStreamContext.ChunkType = 2;
            chunkStreamContext.MessageHeader.Timestamp = messageHeader.TimestampDelta;
            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();

            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, networkStream, cancellationToken);
                messageHeaderSize += extendedTimestampHeader.Size;

                chunkStreamContext.MessageHeader.Timestamp = extendedTimestampHeader.ExtendedTimestamp;
            }

            chunkStreamContext.Timestamp += chunkStreamContext.MessageHeader.Timestamp;

            return messageHeaderSize;
        }

        private async ValueTask<int> HandleChunkMessageHeaderType3Async(
            IRtmpChunkStreamContext chunkStreamContext,
            INetworkStreamReader networkStream,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            int messageHeaderSize = 0;
            var timestampDelta = chunkStreamContext.MessageHeader.Timestamp;

            chunkStreamContext.ChunkType = 3;

            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, networkStream, cancellationToken);
                messageHeaderSize += extendedTimestampHeader.Size;

                timestampDelta = extendedTimestampHeader.ExtendedTimestamp;
            }

            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.Timestamp += timestampDelta;
            }

            return messageHeaderSize;
        }

        private async ValueTask<RtmpChunkMessageAggregationResult> HandleChunkMessagePayloadAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            INetworkStreamReader networkStream,
            uint maxInChunkSize,
            int headerSize,
            CancellationToken cancellationToken)
        {
            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.AssignPayload(_dataBufferPool);
            }

            var payloadBuffer = chunkStreamContext.PayloadBuffer!;
            var messageLength = chunkStreamContext.MessageHeader.MessageLength;
            var chunkedPayloadLength = 0;

            if (payloadBuffer.Size < messageLength)
            {
                chunkedPayloadLength = (int)Math.Min(
                    messageLength - payloadBuffer.Size,
                    maxInChunkSize - payloadBuffer.Size % maxInChunkSize
                );

                await payloadBuffer.AppendStreamData(networkStream, chunkedPayloadLength, cancellationToken);
            }

            if (payloadBuffer.Size == messageLength)
            {
                payloadBuffer.Position = 0;
                return new RtmpChunkMessageAggregationResult(true, headerSize + chunkedPayloadLength, chunkStreamContext);
            }

            return new RtmpChunkMessageAggregationResult(false, headerSize + chunkedPayloadLength, chunkStreamContext);
        }

        public void ResetChunkStreamContext(IRtmpChunkStreamContext chunkStreamContext)
        {
            chunkStreamContext.RecyclePayload(_dataBufferPool);
        }
    }
}
