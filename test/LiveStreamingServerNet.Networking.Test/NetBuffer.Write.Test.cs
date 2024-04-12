using AutoFixture;
using FluentAssertions;

namespace LiveStreamingServerNet.Networking.Test
{
    public class NetBufferWriteTest
    {
        private readonly Fixture _fixture;

        public NetBufferWriteTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void WriteByte()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 1);

            var result = netBuffer.UnderlyingBuffer[startPos];
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteInt16()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<short>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 2);

            var result = BitConverter.ToInt16(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteInt32()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<int>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);
            var endPos = netBuffer.Position;

            // Assert
            endPos.Should().Be(startPos + 4);

            var result = BitConverter.ToInt32(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteInt64()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<long>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 8);

            var result = BitConverter.ToInt64(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteUInt16()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<ushort>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 2);

            var result = BitConverter.ToUInt16(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteUInt32()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<uint>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 4);

            var result = BitConverter.ToUInt32(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteUInt64()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<ulong>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 8);

            var result = BitConverter.ToUInt64(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteSingle()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<float>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 4);

            var result = BitConverter.ToSingle(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteDouble()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<double>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 8);

            var result = BitConverter.ToDouble(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteBool()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<bool>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 1);

            var result = BitConverter.ToBoolean(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteChar()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<char>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 2);

            var result = BitConverter.ToChar(netBuffer.UnderlyingBuffer, startPos);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteUInt16BigEndian()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            ushort expected = _fixture.Create<ushort>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.WriteUint16BigEndian(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 2);

            var result = BitConverter.ToUInt16(netBuffer.UnderlyingBuffer.Skip(startPos).Take(2).Reverse().ToArray(), 0);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteUInt24BigEndian()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            uint expected = _fixture.Create<uint>() & 0xFFFFFF;

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.WriteUInt24BigEndian(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 3);

            var buffer = netBuffer.UnderlyingBuffer.AsSpan(startPos, 3);
            var result = (uint)(buffer[0] << 16 | buffer[1] << 8 | buffer[2]);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteUInt32BigEndian()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            uint expected = _fixture.Create<uint>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.WriteUInt32BigEndian(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 4);

            var result = BitConverter.ToUInt32(netBuffer.UnderlyingBuffer.Skip(startPos).Take(4).Reverse().ToArray(), 0);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteInt16BigEndian()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            short expected = _fixture.Create<short>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.WriteInt16BigEndian(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 2);

            var result = BitConverter.ToInt16(netBuffer.UnderlyingBuffer.Skip(startPos).Take(2).Reverse().ToArray(), 0);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteInt24BigEndian()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            int expected = _fixture.Create<int>() & 0xFFFFFF;

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.WriteInt24BigEndian(expected);

            // Assert
            var buffer = netBuffer.UnderlyingBuffer.AsSpan(startPos, 3);
            var result = (buffer[0] << 16 | buffer[1] << 8 | buffer[2]);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteInt32BigEndian()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            int expected = _fixture.Create<int>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.WriteInt32BigEndian(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 4);

            var result = BitConverter.ToInt32(netBuffer.UnderlyingBuffer.Skip(startPos).Take(4).Reverse().ToArray(), 0);
            result.Should().Be(expected);
        }

        [Fact]
        public void WriteBytes()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + expected.Length);

            var result = netBuffer.UnderlyingBuffer.Skip(startPos).Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void WriteSlice()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var buffer = _fixture.CreateMany<byte>(100).ToArray();
            var expected = buffer.AsSpan(25, 50).ToArray();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(buffer, 25, 50);

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + 50);

            var result = netBuffer.UnderlyingBuffer.Skip(startPos).Take(50);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void WriteSpan()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected.AsSpan());

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + expected.Length);

            var result = netBuffer.UnderlyingBuffer.Skip(startPos).Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void WriteMemory()
        {
            // Arange
            var startPos = _fixture.Create<int>();
            var expected = _fixture.Create<byte[]>();

            // Act
            using var netBuffer = new NetBuffer();
            netBuffer.MoveTo(startPos);
            netBuffer.Write(expected.AsMemory());

            // Assert
            var endPos = netBuffer.Position;
            endPos.Should().Be(startPos + expected.Length);

            var result = netBuffer.UnderlyingBuffer.Skip(startPos).Take(expected.Length);
            result.Should().BeEquivalentTo(expected);
        }
    }
}
