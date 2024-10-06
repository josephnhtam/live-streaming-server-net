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

            var result = dstDataBuffer.AsSpan(0, expected.Length).ToArray();
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

            var result = dstDataBuffer.AsSpan(0, expected.Length).ToArray();
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

            var result = dstDataBuffer.AsSpan(0, expected.Length).ToArray();
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

            var result = dataBuffer.AsSpan(0, expected.Length).ToArray();
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

            var result = dataBuffer.AsSpan(startPos, expected.Length).ToArray();
            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(10, 0)]
        [InlineData(10, 5)]
        [InlineData(10, 8)]
        public void TrimStart(int bufferSize, int trimCount)
        {
            // Arrange
            var buffer = _fixture.CreateMany<byte>(bufferSize).ToArray();

            using var dataBuffer = new DataBuffer();
            dataBuffer.Write(buffer);

            // Act
            dataBuffer.TrimStart(trimCount);

            // Assert
            dataBuffer.AsSpan().ToArray().Should().BeEquivalentTo(buffer.AsSpan(trimCount).ToArray());
        }

        [Theory]
        [InlineData(8, 32, 0, 8)]
        [InlineData(8, 32, 16, 8)]
        [InlineData(8, 32, 16, 16)]
        [InlineData(8, 32, 16, 32)]
        public void TrimStartAndWrite(int initialCapacity, int bufferSize, int trimCount, int secondBufferSize)
        {
            // Arrange
            var buffer = _fixture.CreateMany<byte>(bufferSize).ToArray();
            var secondBuffer = _fixture.CreateMany<byte>(secondBufferSize).ToArray();

            using var dataBuffer = new DataBuffer(initialCapacity);
            dataBuffer.Write(buffer);

            // Act
            dataBuffer.TrimStart(trimCount);
            dataBuffer.Write(secondBuffer);

            // Assert
            dataBuffer.AsSpan().ToArray().Should().BeEquivalentTo(buffer.AsSpan(trimCount).ToArray().Concat(secondBuffer));
        }
    }
}
