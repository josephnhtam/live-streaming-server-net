using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public partial class DataBuffer
    {
        public void ReadBytes(byte[] buffer, int index, int count)
        {
            var targetSpan = buffer.AsSpan(index, count);
            _buffer.AsSpan(_position, count).CopyTo(targetSpan);
            Advance(count);
        }

        public byte[] ReadBytes(int count)
        {
            var result = new byte[count];
            ReadBytes(result, 0, count);
            return result;
        }

        public byte ReadByte()
        {
            return _buffer[_position++];
        }

        public short ReadInt16()
        {
            var value = Unsafe.As<byte, short>(ref _buffer[_position]);
            _position += 2;
            return value;
        }

        public int ReadInt32()
        {
            var value = Unsafe.As<byte, int>(ref _buffer[_position]);
            _position += 4;
            return value;
        }

        public long ReadInt64()
        {
            var value = Unsafe.As<byte, long>(ref _buffer[_position]);
            _position += 8;
            return value;
        }

        public ushort ReadUInt16()
        {
            var value = Unsafe.As<byte, ushort>(ref _buffer[_position]);
            _position += 2;
            return value;
        }

        public uint ReadUInt32()
        {
            var value = Unsafe.As<byte, uint>(ref _buffer[_position]);
            _position += 4;
            return value;
        }

        public ulong ReadUInt64()
        {
            var value = Unsafe.As<byte, ulong>(ref _buffer[_position]);
            _position += 8;
            return value;
        }

        public float ReadSingle()
        {
            var value = Unsafe.As<byte, float>(ref _buffer[_position]);
            _position += 4;
            return value;
        }

        public double ReadDouble()
        {
            var value = Unsafe.As<byte, double>(ref _buffer[_position]);
            _position += 8;
            return value;
        }

        public bool ReadBoolean()
        {
            return Unsafe.As<byte, bool>(ref _buffer[_position++]);
        }

        public char ReadChar()
        {
            var value = Unsafe.As<byte, char>(ref _buffer[_position]);
            _position += 2;
            return value;
        }

        public ushort ReadUInt16BigEndian()
        {
            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ushort>(ref _buffer[_position]));
            _position += 2;
            return value;
        }

        public uint ReadUInt24BigEndian()
        {
            var value = (uint)(_buffer[_position] << 16 | _buffer[_position + 1] << 8 | _buffer[_position + 2]);
            _position += 3;
            return value;
        }

        public uint ReadUInt32BigEndian()
        {
            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, uint>(ref _buffer[_position]));
            _position += 4;
            return value;
        }

        public short ReadInt16BiEndian()
        {
            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, short>(ref _buffer[_position]));
            _position += 2;
            return value;
        }

        public int ReadInt24BigEndian()
        {
            var value = _buffer[_position] << 16 | _buffer[_position + 1] << 8 | _buffer[_position + 2];

            if ((value & 0x800000) != 0)
                value |= unchecked((int)0xff000000);

            _position += 3;
            return value;
        }

        public int ReadInt32BigEndian()
        {
            var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, int>(ref _buffer[_position]));
            _position += 4;
            return value;
        }
    }
}
