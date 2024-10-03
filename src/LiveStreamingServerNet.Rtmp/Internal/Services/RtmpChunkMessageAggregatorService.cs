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

                var success = basicHeader.ChunkType switch
                {
                    0 => await HandleChunkMessageHeaderType0Async(chunkStreamContext, networkStream, headerBuffer, cancellationToken),
                    1 => await HandleChunkMessageHeaderType1Async(chunkStreamContext, networkStream, headerBuffer, cancellationToken),
                    2 => await HandleChunkMessageHeaderType2Async(chunkStreamContext, networkStream, headerBuffer, cancellationToken),
                    3 => await HandleChunkMessageHeaderType3Async(chunkStreamContext, networkStream, headerBuffer, cancellationToken),
                    _ => throw new ArgumentOutOfRangeException(nameof(basicHeader.ChunkType))
                };

                if (!success)
                    return new RtmpChunkMessageAggregationResult(false, 0, chunkStreamContext);

                return await HandleChunkMessagePayloadAsync(
                    chunkStreamContext, networkStream, contextProvider.InChunkSize, headerBuffer.Size, cancellationToken);
            }
            finally
            {
                _dataBufferPool.Recycle(headerBuffer);
            }
        }

        private async ValueTask<bool> HandleChunkMessageHeaderType0Async(
            IRtmpChunkStreamContext chunkStreamContext,
            INetworkStreamReader networkStream,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType0.ReadAsync(dataBuffer, networkStream, cancellationToken);

            chunkStreamContext.ChunkType = 0;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;
            chunkStreamContext.MessageHeader.MessageStreamId = messageHeader.MessageStreamId;

            chunkStreamContext.MessageHeader.TimestampDelta = 0;
            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, networkStream, cancellationToken);
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
            INetworkStreamReader networkStream,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType1.ReadAsync(dataBuffer, networkStream, cancellationToken);

            chunkStreamContext.ChunkType = 1;
            chunkStreamContext.MessageHeader.MessageLength = messageHeader.MessageLength;
            chunkStreamContext.MessageHeader.MessageTypeId = messageHeader.MessageTypeId;

            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, networkStream, cancellationToken);
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
            INetworkStreamReader networkStream,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            var messageHeader = await RtmpChunkMessageHeaderType2.ReadAsync(dataBuffer, networkStream, cancellationToken);

            chunkStreamContext.ChunkType = 2;

            chunkStreamContext.MessageHeader.HasExtendedTimestamp = messageHeader.HasExtendedTimestamp();
            if (chunkStreamContext.MessageHeader.HasExtendedTimestamp)
            {
                var extendedTimestampHeader = await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, networkStream, cancellationToken);
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
            INetworkStreamReader networkStream,
            IDataBuffer dataBuffer,
            CancellationToken cancellationToken)
        {
            chunkStreamContext.ChunkType = 3;

            var timestampDelta = chunkStreamContext.MessageHeader.HasExtendedTimestamp ?
                (await RtmpChunkExtendedTimestampHeader.ReadAsync(dataBuffer, networkStream, cancellationToken)).ExtendedTimestamp :
                chunkStreamContext.MessageHeader.TimestampDelta;

            if (chunkStreamContext.IsFirstChunkOfMessage)
            {
                chunkStreamContext.MessageHeader.TimestampDelta = timestampDelta;
                chunkStreamContext.MessageHeader.Timestamp += timestampDelta;
            }

            return true;
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
                chunkStreamContext.PayloadBuffer = _dataBufferPool.Obtain();
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
            if (chunkStreamContext.PayloadBuffer == null)
                return;

            _dataBufferPool.Recycle(chunkStreamContext.PayloadBuffer);
            chunkStreamContext.PayloadBuffer = null;
        }
    }
}
