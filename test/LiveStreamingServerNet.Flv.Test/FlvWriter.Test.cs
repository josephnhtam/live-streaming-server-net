using AutoFixture;
using LiveStreamingServerNet.Flv.Internal;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test
{
    public class FlvWriterTest
    {
        private readonly IFixture _fixture;
        private readonly IStreamWriter _streamWriter;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IDataBuffer _dataBuffer;
        private readonly FlvWriter _sut;

        public FlvWriterTest()
        {
            _fixture = new Fixture();
            _streamWriter = Substitute.For<IStreamWriter>();
            _dataBufferPool = Substitute.For<IDataBufferPool>();
            _dataBuffer = new DataBuffer();
            _dataBufferPool.Obtain().Returns(_dataBuffer);
            _sut = new FlvWriter(_streamWriter, _dataBufferPool);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task WriteHeaderAsync_Should_WriteHeaderToStreamWriter(bool allowAudioTags, bool allowVideoTags)
        {
            // Act
            await _sut.WriteHeaderAsync(allowAudioTags, allowVideoTags, default);

            // Assert
            await _streamWriter.Received(1).WriteAsync(
                Arg.Is<ReadOnlyMemory<byte>>(bytes => VerifyHeaderBytes(bytes, allowAudioTags, allowVideoTags)),
                Arg.Any<CancellationToken>());
        }

        private static bool VerifyHeaderBytes(ReadOnlyMemory<byte> bytes, bool allowAudioTags, bool allosVideoTags)
        {
            var expectedBytes = new byte[]
            {
                0x46, 0x4c, 0x56, 0x01,
                (byte)((allowAudioTags ? 0x04 : 0) | (allosVideoTags ? 0x01 : 0)),
                0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00
            };

            return bytes.ToArray().SequenceEqual(expectedBytes);
        }

        [Theory]
        [InlineData(FlvTagType.Audio)]
        [InlineData(FlvTagType.Video)]
        [InlineData(FlvTagType.ScriptData)]
        internal async Task WriteTagAsync_Should_WriteTagToStreamWriter(FlvTagType tagType)
        {
            // Arrange
            var timestamp = _fixture.Create<uint>();
            var payload = _fixture.Create<byte[]>();

            // Act
            await _sut.WriteTagAsync(tagType, timestamp, dataBuffer => dataBuffer.Write(payload), default);

            // Assert
            await _streamWriter.Received(1).WriteAsync(
                Arg.Is<ReadOnlyMemory<byte>>(x => VerifyTagBytes(x, tagType, timestamp, payload)),
                Arg.Any<CancellationToken>());
        }

        private static bool VerifyTagBytes(ReadOnlyMemory<byte> bytes, FlvTagType tagType, uint timestamp, byte[] payload)
        {
            using var dataBuffer = new DataBuffer();

            var header = new FlvTagHeader(tagType, (uint)payload.Length, timestamp);
            header.Write(dataBuffer);
            dataBuffer.Write(payload);
            dataBuffer.WriteUInt32BigEndian((uint)dataBuffer.Size);

            return bytes.ToArray().SequenceEqual(dataBuffer.UnderlyingBuffer.Take(dataBuffer.Size).ToArray());
        }
    }
}
