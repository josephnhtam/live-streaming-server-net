using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpChunkStreamContext : IRtmpChunkStreamContext
    {
        public uint ChunkStreamId { get; }
        public int ChunkType { get; set; }
        public bool IsFirstChunkOfMessage => PayloadBuffer == null;
        public IRtmpChunkMessageHeaderContext MessageHeader { get; }
        public IDataBuffer? PayloadBuffer { get; private set; }
        public uint Timestamp { get; set; }

        public RtmpChunkStreamContext(uint chunkStreamId)
        {
            ChunkStreamId = chunkStreamId;
            MessageHeader = new RtmpChunkMessageHeaderContext();
        }

        public void AssignPayload(IDataBufferPool dataBufferPool)
        {
            if (PayloadBuffer != null)
                return;

            PayloadBuffer = dataBufferPool.Obtain();
        }

        public void RecyclePayload(IDataBufferPool dataBufferPool)
        {
            if (PayloadBuffer == null)
                return;

            dataBufferPool.Recycle(PayloadBuffer);
            PayloadBuffer = null;
        }
    }

    internal class RtmpChunkMessageHeaderContext : IRtmpChunkMessageHeaderContext
    {
        public uint Timestamp { get; set; }
        public uint TimestampDelta { get; set; }
        public int MessageLength { get; set; }
        public byte MessageTypeId { get; set; }
        public uint MessageStreamId { get; set; }
        public bool HasExtendedTimestamp { get; set; }
    }
}
