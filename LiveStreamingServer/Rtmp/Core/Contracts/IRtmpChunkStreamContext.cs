namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpChunkStreamContext
    {
        uint ChunkStreamId { get; }
        int ChunkType { get; set; }
        bool IsFirstChunkOfMessage { get; set; }
        IRtmpChunkMessageHeaderContext MessageHeader { get; }
    }

    public interface IRtmpChunkMessageHeaderContext
    {
        uint Timestamp { get; set; }
        uint TimestampDelta { get; set; }
        uint MessageLength { get; set; }
        int MessageTypeId { get; set; }
        uint MessageStreamId { get; set; }
        bool HasExtendedTimestamp { get; set; }
    }
}
