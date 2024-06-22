using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal record struct FlvTagHeader(FlvTagType tagType, uint dataSize, uint timestamp)
    {
        public const int Size = 11;

        public void Write(IDataBuffer dataBuffer)
        {
            dataBuffer.Write((byte)tagType);
            dataBuffer.WriteUInt24BigEndian(dataSize);
            dataBuffer.WriteUInt24BigEndian(timestamp);
            dataBuffer.WriteUInt32BigEndian(0);
        }
    }

    internal enum FlvTagType : byte
    {
        Audio = 8,
        Video = 9,
        ScriptData = 18
    }
}
