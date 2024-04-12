using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Networking
{
    public partial class NetBuffer
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
            Advance(1);
            return _buffer[_position - 1];
        }

        public short ReadInt16()
        {
            Advance(2);
            return Unsafe.As<byte, short>(ref _buffer[_position - 2]);
        }

        public int ReadInt32()
        {
            Advance(4);
            return Unsafe.As<byte, int>(ref _buffer[_position - 4]);
        }

        public long ReadInt64()
        {
            Advance(8);
            return Unsafe.As<byte, long>(ref _buffer[_position - 8]);
        }

        public ushort ReadUInt16()
        {
            Advance(2);
            return Unsafe.As<byte, ushort>(ref _buffer[_position - 2]);
        }

        public uint ReadUInt32()
        {
            Advance(4);
            return Unsafe.As<byte, uint>(ref _buffer[_position - 4]);
        }

        public ulong ReadUInt64()
        {
            Advance(8);
            return Unsafe.As<byte, ulong>(ref _buffer[_position - 8]);
        }

        public float ReadSingle()
        {
            Advance(4);
            return Unsafe.As<byte, float>(ref _buffer[_position - 4]);
        }

        public double ReadDouble()
        {
            Advance(8);
            return Unsafe.As<byte, double>(ref _buffer[_position - 8]);
        }

        public bool ReadBoolean()
        {
            Advance(1);
            return Unsafe.As<byte, bool>(ref _buffer[_position - 1]);
        }

        public char ReadChar()
        {
            Advance(2);
            return Unsafe.As<byte, char>(ref _buffer[_position - 2]);
        }

        public ushort ReadUInt16BigEndian()
        {
            Advance(2);
            return BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ushort>(ref _buffer[_position - 2]));
        }

        public uint ReadUInt24BigEndian()
        {
            var value = (uint)(_buffer[_position] << 16 | _buffer[_position + 1] << 8 | _buffer[_position + 2]);
            Advance(3);
            return value;
        }

        public uint ReadUInt32BigEndian()
        {
            Advance(4);
            return BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, uint>(ref _buffer[_position - 4]));
        }

        public short ReadInt16BiEndian()
        {
            Advance(2);
            return BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, short>(ref _buffer[_position - 2]));
        }

        public int ReadInt24BigEndian()
        {
            var value = _buffer[_position] << 16 | _buffer[_position + 1] << 8 | _buffer[_position + 2];

            if ((value & 0x800000) != 0)
                value |= unchecked((int)0xff000000);

            Advance(3);
            return value;
        }

        public int ReadInt32BigEndian()
        {
            Advance(4);
            return BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, int>(ref _buffer[_position - 4]));
        }
    }
}
