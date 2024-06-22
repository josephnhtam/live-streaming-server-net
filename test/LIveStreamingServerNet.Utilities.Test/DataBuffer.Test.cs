using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Utilities.Buffers;

namespace LIveStreamingServerNet.Utilities.Test
{
    public class DataBufferTest
    {
        private readonly IFixture _fixture;

        public DataBufferTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void MoveTo()
        {
            // Arange
            var expected = _fixture.Create<int>();

            // Act
            using var dataBuffer = new DataBuffer();
            dataBuffer.MoveTo(expected);

            // Assert
            var result = dataBuffer.Position;
            result.Should().Be(expected);
        }

        [Fact]
        public void Position()
        {
            // Arange
            var expected = _fixture.Create<int>();

            // Act
            using var dataBuffer = new DataBuffer();
            dataBuffer.Position = expected;

            // Assert
            var result = dataBuffer.Position;
            result.Should().Be(expected);
        }

        [Fact]
        public void Size()
        {
            // Arange
            var expected = _fixture.Create<int>();

            // Act
            using var dataBuffer = new DataBuffer();
            dataBuffer.Size = expected;

            // Assert
            var result = dataBuffer.Size;
            result.Should().Be(expected);
        }

        [Fact]
        public void Reset()
        {
            // Arange
            using var dataBuffer = new DataBuffer();
            dataBuffer.Size = _fixture.Create<int>();
            dataBuffer.Position = _fixture.Create<int>();

            // Act
            dataBuffer.Reset();

            // Assert
            var pos = dataBuffer.Position;
            pos.Should().Be(0);

            var size = dataBuffer.Size;
            size.Should().Be(0);
        }

        [Fact]
        public void FlushToDataBuffer()
        {
            // Arange
            var expected = _fixture.Create<byte[]>();

            using var srcDataBuffer = new DataBuffer();
            srcDataBuffer.Write(expected);

            // Act
            using var dstDataBuffer = new DataBuffer();
            srcDataBuffer.Flush(dstDataBuffer);

            // Assert
            srcDataBuffer.Position.Should().Be(0);

            var result = dstDataBuffer.UnderlyingBuffer.Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void FlushToStream()
        {
            // Arange
            var expected = _fixture.Create<byte[]>();

            using var srcDataBuffer = new DataBuffer();
            srcDataBuffer.Write(expected);

            using var stream = new MemoryStream();

            // Act
            srcDataBuffer.Flush(stream);

            // Assert
            srcDataBuffer.Position.Should().Be(0);

            var result = stream.ToArray();
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task FlushToStreamAsync()
        {
            // Arange
            var expected = _fixture.Create<byte[]>();

            using var srcDataBuffer = new DataBuffer();
            srcDataBuffer.Write(expected);

            using var stream = new MemoryStream();

            // Act
            await srcDataBuffer.FlushAsync(stream);

            // Assert
            srcDataBuffer.Position.Should().Be(0);

            var result = stream.ToArray();
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void CopyAllTo()
        {
            // Arange
            var expected = _fixture.Create<byte[]>();

            using var srcDataBuffer = new DataBuffer();
            srcDataBuffer.Write(expected);

            var expectedPos = srcDataBuffer.Position;

            // Act
            using var dstDataBuffer = new DataBuffer();
            srcDataBuffer.CopyAllTo(dstDataBuffer);

            // Assert
            srcDataBuffer.Position.Should().Be(expectedPos);

            var result = dstDataBuffer.UnderlyingBuffer.Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ReadAndWriteTo()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            using var srcDataBuffer = new DataBuffer();
            srcDataBuffer.MoveTo(startPos).Write(expected);
            srcDataBuffer.MoveTo(startPos);

            var expectedEndPos = startPos + expected.Length;

            // Act
            using var dstDataBuffer = new DataBuffer();
            srcDataBuffer.ReadAndWriteTo(dstDataBuffer, expected.Length);

            // Assert
            srcDataBuffer.Position.Should().Be(expectedEndPos);

            var result = dstDataBuffer.UnderlyingBuffer.Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task FromStreamData()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            // Act
            using var dataBuffer = new DataBuffer();
            using var stream = new MemoryStream(expected);
            dataBuffer.MoveTo(startPos);
            await dataBuffer.FromStreamData(stream, expected.Length);

            // Assert
            var size = dataBuffer.Size;
            size.Should().Be(expected.Length);

            var result = dataBuffer.UnderlyingBuffer.Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task AppendStreamData()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            // Act
            using var dataBuffer = new DataBuffer();
            using var stream = new MemoryStream(expected);
            dataBuffer.MoveTo(startPos);
            await dataBuffer.AppendStreamData(stream, expected.Length);

            // Assert
            var size = dataBuffer.Size;
            size.Should().Be(startPos + expected.Length);

            var result = dataBuffer.UnderlyingBuffer.Skip(startPos).Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }
    }
}
