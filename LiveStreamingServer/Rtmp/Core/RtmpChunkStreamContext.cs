using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpChunkStreamContext : IRtmpChunkStreamContext
    {
        public uint ChunkStreamId { get; }
        public int ChunkType { get; set; }
        public bool IsFirstChunkOfMessage { get; set; }
        public IRtmpChunkMessageHeaderContext MessageHeader { get; }

        public RtmpChunkStreamContext(uint chunkStreamId)
        {
            ChunkStreamId = chunkStreamId;
            IsFirstChunkOfMessage = true;
            MessageHeader = new RtmpChunkMessageHeaderContext();
        }
    }

    public struct RtmpChunkMessageHeaderContext : IRtmpChunkMessageHeaderContext
    {
        public uint Timestamp { get; set; }
        public uint TimestampDelta { get; set; }
        public uint MessageLength { get; set; }
        public int MessageTypeId { get; set; }
        public uint MessageStreamId { get; set; }
        public bool HasExtendedTimestamp { get; set; }
    }
}
