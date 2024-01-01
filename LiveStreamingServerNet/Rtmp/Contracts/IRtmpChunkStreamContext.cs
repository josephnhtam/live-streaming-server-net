using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpChunkStreamContext
    {
        uint ChunkStreamId { get; }
        int ChunkType { get; set; }
        bool IsFirstChunkOfMessage { get; }
        IRtmpChunkMessageHeaderContext MessageHeader { get; }
        INetBuffer? PayloadBuffer { get; set; }
    }

    public interface IRtmpChunkMessageHeaderContext
    {
        uint Timestamp { get; set; }
        uint TimestampDelta { get; set; }
        int MessageLength { get; set; }
        byte MessageTypeId { get; set; }
        uint MessageStreamId { get; set; }
        bool HasExtendedTimestamp { get; set; }
    }
}
