using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.RtmpClientPlayDemo
{
    public class FlvWriter : IDisposable
    {
        private readonly BinaryWriter _streamWriter;

        public FlvWriter(BinaryWriter streamWriter)
        {
            _streamWriter = streamWriter;
        }

        public void WriteHeader(bool allowAudioTags, bool allowVideoTags)
        {
            byte typeFlags = 0;

            if (allowAudioTags)
                typeFlags |= 0x04;

            if (allowVideoTags)
                typeFlags |= 0x01;

            _streamWriter.Write([0x46, 0x4c, 0x56, 0x01, typeFlags, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00]);
        }

        public void WriteTag(FlvTagType tagType, uint timestamp, ReadOnlySpan<byte> payload)
        {
            using var dataBuffer = new DataBuffer();

            var payloadSize = (uint)payload.Length;
            var packetSize = payloadSize + FlvTagHeader.Size;

            var header = new FlvTagHeader(tagType, payloadSize, timestamp);

            header.Write(dataBuffer);
            dataBuffer.Write(payload);
            dataBuffer.WriteUInt32BigEndian(packetSize);

            _streamWriter.Write(dataBuffer.AsSpan());
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
        }

        private record struct FlvTagHeader(FlvTagType TagType, uint DataSize, uint Timestamp)
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
    }

    public enum FlvTagType : byte
    {
        Audio = 8,
        Video = 9,
        ScriptData = 18
    }
}
