using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpChunkMessageAggregatorService
    {
        ValueTask<RtmpChunkMessageAggregationResult> AggregateChunkMessagesAsync(
            INetworkStreamReader networkStream,
            IRtmpChunkStreamContextProvider contextProvider,
            CancellationToken cancellationToken);

        void ResetChunkStreamContext(IRtmpChunkStreamContext chunkStreamContext);
    }

    internal record struct RtmpChunkMessageAggregationResult(bool IsComplete, int ChunkMessageSize, IRtmpChunkStreamContext ChunkStreamContext);
}
