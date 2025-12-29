using LiveStreamingServerNet.Utilities.Buffers.Internal;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public partial class DataBuffer
    {
        public void ReadBytes(byte[] buffer, int index, int count)
        {
            DataBufferRead.ReadBytes(_buffer, _startIndex, _size, ref _position, buffer, index, count);
        }

        public void ReadBytes(Span<byte> buffer)
        {
            DataBufferRead.ReadBytes(_buffer, _startIndex, _size, ref _position, buffer);
        }

        public byte[] ReadBytes(int count)
        {
            return DataBufferRead.ReadBytes(_buffer, _startIndex, _size, ref _position, count);
        }

        public byte ReadByte()
        {
            return DataBufferRead.ReadByte(_buffer, _startIndex, _size, ref _position);
        }

        public short ReadInt16()
        {
            return DataBufferRead.ReadInt16(_buffer, _startIndex, _size, ref _position);
        }

        public int ReadInt32()
        {
            return DataBufferRead.ReadInt32(_buffer, _startIndex, _size, ref _position);
        }

        public long ReadInt64()
        {
            return DataBufferRead.ReadInt64(_buffer, _startIndex, _size, ref _position);
        }

        public ushort ReadUInt16()
        {
            return DataBufferRead.ReadUInt16(_buffer, _startIndex, _size, ref _position);
        }

        public uint ReadUInt32()
        {
            return DataBufferRead.ReadUInt32(_buffer, _startIndex, _size, ref _position);
        }

        public ulong ReadUInt64()
        {
            return DataBufferRead.ReadUInt64(_buffer, _startIndex, _size, ref _position);
        }

        public float ReadSingle()
        {
            return DataBufferRead.ReadSingle(_buffer, _startIndex, _size, ref _position);
        }

        public double ReadDouble()
        {
            return DataBufferRead.ReadDouble(_buffer, _startIndex, _size, ref _position);
        }

        public bool ReadBoolean()
        {
            return DataBufferRead.ReadBoolean(_buffer, _startIndex, _size, ref _position);
        }

        public char ReadChar()
        {
            return DataBufferRead.ReadChar(_buffer, _startIndex, _size, ref _position);
        }

        public ushort ReadUInt16BigEndian()
        {
            return DataBufferRead.ReadUInt16BigEndian(_buffer, _startIndex, _size, ref _position);
        }

        public uint ReadUInt24BigEndian()
        {
            return DataBufferRead.ReadUInt24BigEndian(_buffer, _startIndex, _size, ref _position);
        }

        public uint ReadUInt32BigEndian()
        {
            return DataBufferRead.ReadUInt32BigEndian(_buffer, _startIndex, _size, ref _position);
        }

        public ulong ReadUInt64BigEndian()
        {
            return DataBufferRead.ReadUInt64BigEndian(_buffer, _startIndex, _size, ref _position);
        }

        public short ReadInt16BigEndian()
        {
            return DataBufferRead.ReadInt16BigEndian(_buffer, _startIndex, _size, ref _position);
        }

        public int ReadInt24BigEndian()
        {
            return DataBufferRead.ReadInt24BigEndian(_buffer, _startIndex, _size, ref _position);
        }

        public int ReadInt32BigEndian()
        {
            return DataBufferRead.ReadInt32BigEndian(_buffer, _startIndex, _size, ref _position);
        }

        public long ReadInt64BigEndian()
        {
            return DataBufferRead.ReadInt64BigEndian(_buffer, _startIndex, _size, ref _position);
        }

        public string ReadUtf8String(int length)
        {
            return DataBufferRead.ReadUtf8String(_buffer, _startIndex, _size, ref _position, length);
        }
    }
}
