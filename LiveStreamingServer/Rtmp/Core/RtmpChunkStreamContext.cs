using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpChunkStreamContext : IRtmpChunkStreamContext
    {
        public uint ChunkStreamId { get; }
        public int ChunkType { get; set; }
        public bool IsFirstChunkOfMessage => PayloadBuffer == null;
        public IRtmpChunkMessageHeaderContext MessageHeader { get; }
        public INetBuffer? PayloadBuffer { get; set; }

        public RtmpChunkStreamContext(uint chunkStreamId)
        {
            ChunkStreamId = chunkStreamId;
            MessageHeader = new RtmpChunkMessageHeaderContext();
        }
    }

    public struct RtmpChunkMessageHeaderContext : IRtmpChunkMessageHeaderContext
    {
        public uint Timestamp { get; set; }
        public uint TimestampDelta { get; set; }
        public int MessageLength { get; set; }
        public int MessageTypeId { get; set; }
        public uint MessageStreamId { get; set; }
        public bool HasExtendedTimestamp { get; set; }
    }
}
