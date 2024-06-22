using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Utilities.Buffers;

namespace LIveStreamingServerNet.Utilities.Test
{
    public class DataBufferReadTest
    {
        private readonly IFixture _fixture;

        public DataBufferReadTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ReadBytes()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expected, 0, expected.Length);

            // Act
            var result = buffer.MoveTo(startPos).ReadBytes(expected.Length);

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + expected.Length);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ReadBytesIntoSlice()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.CreateMany<byte>(100).ToArray();
            var slice = new byte[25 + expected.Length];

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expected, 0, expected.Length);

            // Act
            buffer.MoveTo(startPos).ReadBytes(slice, 25, expected.Length);

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + expected.Length);

            slice.AsSpan(25).ToArray().Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ReadByte()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte>();

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expected);

            // Act
            var result = buffer.MoveTo(startPos).ReadByte();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(byte));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadInt16()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<short>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadInt16();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(short));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadInt32()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<int>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadInt32();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(int));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadInt64()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<long>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadInt64();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(long));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadSingle()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<float>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadSingle();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(float));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadDouble()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<double>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadDouble();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(double));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadBoolean()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<bool>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadBoolean();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(bool));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadChar()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<char>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadChar();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(char));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadUInt16()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<ushort>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadUInt16();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(ushort));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadUInt32()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<uint>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadUInt32();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(uint));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadUInt64()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<ulong>();
            var expectedBytes = BitConverter.GetBytes(expected);

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadUInt64();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(ulong));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadUInt16BigEndian()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<ushort>();
            var expectedBytes = BitConverter.GetBytes(expected).Reverse().ToArray();

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadUInt16BigEndian();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(ushort));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadUInt24BigEndian()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<uint>();
            var expectedBytes = BitConverter.GetBytes(expected).Take(3).Reverse().ToArray();

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadUInt24BigEndian();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + 3);

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadUInt32BigEndian()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<uint>();
            var expectedBytes = BitConverter.GetBytes(expected).Reverse().ToArray();

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadUInt32BigEndian();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(uint));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadInt16BiEndian()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<short>();
            var expectedBytes = BitConverter.GetBytes(expected).Reverse().ToArray();

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadInt16BiEndian();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(short));

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadInt24BigEndian()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<int>();
            var expectedBytes = BitConverter.GetBytes(expected).Take(3).Reverse().ToArray();

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadInt24BigEndian();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + 3);

            result.Should().Be(expected);
        }

        [Fact]
        public void ReadInt32BigEndian()
        {
            // Arrange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<int>();
            var expectedBytes = BitConverter.GetBytes(expected).Reverse().ToArray();

            using var buffer = new DataBuffer();
            buffer.MoveTo(startPos).Write(expectedBytes);

            // Act
            var result = buffer.MoveTo(startPos).ReadInt32BigEndian();

            // Assert
            var endPos = buffer.Position;
            endPos.Should().Be(startPos + sizeof(int));

            result.Should().Be(expected);
        }
    }
}
