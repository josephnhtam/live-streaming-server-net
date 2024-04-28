using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal record struct FlvTagHeader(FlvTagType tagType, uint dataSize, uint timestamp)
    {
        public const int Size = 11;

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.Write((byte)tagType);
            netBuffer.WriteUInt24BigEndian(dataSize);
            netBuffer.WriteUInt24BigEndian(timestamp);
            netBuffer.WriteUInt32BigEndian(0);
        }
    }

    internal enum FlvTagType : byte
    {
        Audio = 8,
        Video = 9,
        ScriptData = 18
    }
}
