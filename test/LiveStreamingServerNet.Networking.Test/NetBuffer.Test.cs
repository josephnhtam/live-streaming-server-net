using AutoFixture;
using FluentAssertions;

namespace LiveStreamingServerNet.Networking.Test
{
    public class NetBufferTest
    {
        private readonly IFixture _fixture;

        public NetBufferTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void MoveTo()
        {
            // Arange
            var expected = _fixture.Create<int>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(expected);

            // Assert
            var result = netBuffer.Position;
            result.Should().Be(expected);
        }

        [Fact]
        public void Position()
        {
            // Arange
            var expected = _fixture.Create<int>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.Position = expected;

            // Assert
            var result = netBuffer.Position;
            result.Should().Be(expected);
        }

        [Fact]
        public void Size()
        {
            // Arange
            var expected = _fixture.Create<int>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.Size = expected;

            // Assert
            var result = netBuffer.Size;
            result.Should().Be(expected);
        }

        [Fact]
        public void Reset()
        {
            // Arange
            using var netBuffer = new NetBuffer();
            netBuffer.Size = _fixture.Create<int>();
            netBuffer.Position = _fixture.Create<int>();

            // Act
            netBuffer.Reset();

            // Assert
            var pos = netBuffer.Position;
            pos.Should().Be(0);

            var size = netBuffer.Size;
            size.Should().Be(0);
        }

        [Fact]
        public void FlushToNetBuffer()
        {
            // Arange
            var expected = _fixture.Create<byte[]>();

            using var srcNetBuffer = new NetBuffer();
            srcNetBuffer.Write(expected);

            // Act
            using var dstNetBuffer = new NetBuffer();
            srcNetBuffer.Flush(dstNetBuffer);

            // Assert
            srcNetBuffer.Position.Should().Be(0);

            var result = dstNetBuffer.UnderlyingBuffer.Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void FlushToStream()
        {
            // Arange
            var expected = _fixture.Create<byte[]>();

            using var srcNetBuffer = new NetBuffer();
            srcNetBuffer.Write(expected);

            using var stream = new MemoryStream();

            // Act
            srcNetBuffer.Flush(stream);

            // Assert
            srcNetBuffer.Position.Should().Be(0);

            var result = stream.ToArray();
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task FlushToStreamAsync()
        {
            // Arange
            var expected = _fixture.Create<byte[]>();

            using var srcNetBuffer = new NetBuffer();
            srcNetBuffer.Write(expected);

            using var stream = new MemoryStream();

            // Act
            await srcNetBuffer.FlushAsync(stream);

            // Assert
            srcNetBuffer.Position.Should().Be(0);

            var result = stream.ToArray();
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void CopyAllTo()
        {
            // Arange
            var expected = _fixture.Create<byte[]>();

            using var srcNetBuffer = new NetBuffer();
            srcNetBuffer.Write(expected);

            var expectedPos = srcNetBuffer.Position;

            // Act
            using var dstNetBuffer = new NetBuffer();
            srcNetBuffer.CopyAllTo(dstNetBuffer);

            // Assert
            srcNetBuffer.Position.Should().Be(expectedPos);

            var result = dstNetBuffer.UnderlyingBuffer.Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ReadAndWriteTo()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            using var srcNetBuffer = new NetBuffer();
            srcNetBuffer.MoveTo(startPos).Write(expected);
            srcNetBuffer.MoveTo(startPos);

            var expectedEndPos = startPos + expected.Length;

            // Act
            using var dstNetBuffer = new NetBuffer();
            srcNetBuffer.ReadAndWriteTo(dstNetBuffer, expected.Length);

            // Assert
            srcNetBuffer.Position.Should().Be(expectedEndPos);

            var result = dstNetBuffer.UnderlyingBuffer.Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task FromStreamData()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            // Act
            using var netBuffer = new NetBuffer();
            using var stream = new MemoryStream(expected);
            netBuffer.MoveTo(startPos);
            await netBuffer.FromStreamData(stream, expected.Length);

            // Assert
            var size = netBuffer.Size;
            size.Should().Be(expected.Length);

            var result = netBuffer.UnderlyingBuffer.Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task AppendStreamData()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            // Act
            using var netBuffer = new NetBuffer();
            using var stream = new MemoryStream(expected);
            netBuffer.MoveTo(startPos);
            await netBuffer.AppendStreamData(stream, expected.Length);

            // Assert
            var size = netBuffer.Size;
            size.Should().Be(startPos + expected.Length);

            var result = netBuffer.UnderlyingBuffer.Skip(startPos).Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }
    }
}
