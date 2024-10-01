using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal record struct FlvTagHeader(FlvTagType TagType, uint DataSize, uint Timestamp)
    {
        public const int Size = 11;

        public void Write(IDataBuffer dataBuffer)
        {
            dataBuffer.Write((byte)TagType);
            dataBuffer.WriteUInt24BigEndian(DataSize);
            dataBuffer.WriteUInt24BigEndian(Timestamp);
            dataBuffer.Write((byte)(Timestamp >> 24));
            dataBuffer.WriteUInt24BigEndian(0);
        }
    }

    internal enum FlvTagType : byte
    {
        Audio = 8,
        Video = 9,
        ScriptData = 18
    }
}
