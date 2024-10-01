using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.RtmpClientPublishDemo
{
    public class FlvReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly IDataBuffer _buffer;

        public FlvReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _buffer = new DataBuffer();
        }

        public async ValueTask<FlvHeader?> ReadHeaderAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _buffer.FromStreamData(_stream, 9, cancellationToken);

                var signature = _buffer.ReadBytes(3);
                if (signature[0] != 'F' || signature[1] != 'L' || signature[2] != 'V')
                    throw new InvalidDataException("Invalid FLV signature.");

                var version = _buffer.ReadByte();
                var flags = _buffer.ReadByte();
                var headerSize = _buffer.ReadInt32BigEndian();

                var header = new FlvHeader(version, flags, headerSize);

                if (headerSize > 9)
                    await _buffer.FromStreamData(_stream, headerSize - 9, cancellationToken);

                await _buffer.FromStreamData(_stream, 4, cancellationToken);
                var previousTagSize = _buffer.ReadUInt32BigEndian();

                if (previousTagSize != 0)
                    throw new InvalidDataException("Invalid PreviousTagSize.");

                return header;
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        public async ValueTask<FlvTag?> ReadTagAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _buffer.FromStreamData(_stream, FlvTagHeader.Size, cancellationToken);

                var tagHeader = FlvTagHeader.Read(_buffer);

                await _buffer.FromStreamData(_stream, (int)tagHeader.DataSize, cancellationToken);
                var payload = _buffer.ToRentedBuffer();

                await _buffer.FromStreamData(_stream, 4, cancellationToken);
                var previousTagSize = _buffer.ReadUInt32BigEndian();

                if (previousTagSize != (11 + tagHeader.DataSize))
                    throw new InvalidDataException("Mismatch in PreviousTagSize.");

                return new FlvTag(tagHeader, payload);
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }
    }

    public enum FlvTagType : byte
    {
        Audio = 8,
        Video = 9,
        ScriptData = 18
    }

    public record struct FlvHeader(byte Version, byte Flags, int HeaderSize)
    {
        public bool HasAudio => (Flags & 0x04) != 0;
        public bool HasVideo => (Flags & 0x01) != 0;
    }

    public record struct FlvTagHeader(FlvTagType TagType, uint DataSize, uint Timestamp)
    {
        public const int Size = 11;

        public static FlvTagHeader Read(IDataBuffer dataBuffer)
        {
            var tagType = (FlvTagType)dataBuffer.ReadByte();
            var dataSize = dataBuffer.ReadUInt24BigEndian();

            var timestampLower = dataBuffer.ReadUInt24BigEndian();
            var timestampExtended = dataBuffer.ReadByte();
            var timestamp = ((uint)timestampExtended << 24) | timestampLower;

            dataBuffer.ReadUInt24BigEndian();

            return new FlvTagHeader(tagType, dataSize, timestamp);
        }
    }

    public record FlvTag(FlvTagHeader Header, IRentedBuffer Payload);
}
