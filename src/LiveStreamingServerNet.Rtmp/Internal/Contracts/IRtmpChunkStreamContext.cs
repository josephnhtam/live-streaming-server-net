using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpChunkStreamContext
    {
        uint ChunkStreamId { get; }
        int ChunkType { get; set; }
        bool IsFirstChunkOfMessage { get; }
        IRtmpChunkMessageHeaderContext MessageHeader { get; }
        IDataBuffer? PayloadBuffer { get; set; }
    }

    internal interface IRtmpChunkMessageHeaderContext
    {
        uint Timestamp { get; set; }
        uint TimestampDelta { get; set; }
        int MessageLength { get; set; }
        byte MessageTypeId { get; set; }
        uint MessageStreamId { get; set; }
        bool HasExtendedTimestamp { get; set; }
    }
}
