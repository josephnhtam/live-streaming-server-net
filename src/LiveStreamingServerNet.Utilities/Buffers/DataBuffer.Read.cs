using LiveStreamingServerNet.Utilities.Buffers.Internal;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public partial class DataBuffer
    {
        public void ReadBytes(byte[] buffer, int index, int count)
            => DataBufferRead.ReadBytes(_buffer, _startIndex, _size, ref _position, buffer, index, count);

        public void ReadBytes(Span<byte> buffer)
            => DataBufferRead.ReadBytes(_buffer, _startIndex, _size, ref _position, buffer);

        public byte[] ReadBytes(int count)
            => DataBufferRead.ReadBytes(_buffer, _startIndex, _size, ref _position, count);

        public byte ReadByte()
            => DataBufferRead.ReadByte(_buffer, _startIndex, _size, ref _position);

        public short ReadInt16()
            => DataBufferRead.ReadInt16(_buffer, _startIndex, _size, ref _position);

        public int ReadInt32()
            => DataBufferRead.ReadInt32(_buffer, _startIndex, _size, ref _position);

        public long ReadInt64()
            => DataBufferRead.ReadInt64(_buffer, _startIndex, _size, ref _position);

        public ushort ReadUInt16()
            => DataBufferRead.ReadUInt16(_buffer, _startIndex, _size, ref _position);

        public uint ReadUInt32()
            => DataBufferRead.ReadUInt32(_buffer, _startIndex, _size, ref _position);

        public ulong ReadUInt64()
            => DataBufferRead.ReadUInt64(_buffer, _startIndex, _size, ref _position);

        public float ReadSingle()
            => DataBufferRead.ReadSingle(_buffer, _startIndex, _size, ref _position);

        public double ReadDouble()
            => DataBufferRead.ReadDouble(_buffer, _startIndex, _size, ref _position);

        public bool ReadBoolean()
            => DataBufferRead.ReadBoolean(_buffer, _startIndex, _size, ref _position);

        public char ReadChar()
            => DataBufferRead.ReadChar(_buffer, _startIndex, _size, ref _position);

        public ushort ReadUInt16BigEndian()
            => DataBufferRead.ReadUInt16BigEndian(_buffer, _startIndex, _size, ref _position);

        public uint ReadUInt24BigEndian()
            => DataBufferRead.ReadUInt24BigEndian(_buffer, _startIndex, _size, ref _position);

        public uint ReadUInt32BigEndian()
            => DataBufferRead.ReadUInt32BigEndian(_buffer, _startIndex, _size, ref _position);

        public ulong ReadUInt64BigEndian()
            => DataBufferRead.ReadUInt64BigEndian(_buffer, _startIndex, _size, ref _position);

        public ulong ReadUInt64BigEndian()
        {
            EnsureRemainingSize(8);

            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ulong>(ref _buffer[_startIndex + _position]));
            _position += 8;
            return value;
        }

        public short ReadInt16BigEndian()
            => DataBufferRead.ReadInt16BigEndian(_buffer, _startIndex, _size, ref _position);

        public int ReadInt24BigEndian()
            => DataBufferRead.ReadInt24BigEndian(_buffer, _startIndex, _size, ref _position);

        public int ReadInt32BigEndian()
            => DataBufferRead.ReadInt32BigEndian(_buffer, _startIndex, _size, ref _position);

        public long ReadInt64BigEndian()
            => DataBufferRead.ReadInt64BigEndian(_buffer, _startIndex, _size, ref _position);

        public long ReadInt64BigEndian()
        {
            EnsureRemainingSize(8);

            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, long>(ref _buffer[_startIndex + _position]));
            _position += 8;
            return value;
        }

        public string ReadUtf8String(int length)
            => DataBufferRead.ReadUtf8String(_buffer, _startIndex, _size, ref _position, length);
    }
}
