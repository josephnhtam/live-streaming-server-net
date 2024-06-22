using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public partial class DataBuffer
    {
        public void WriteRandomBytes(int count)
        {
            var pos = _position;
            Advance(count);

            for (int i = 0; i < count; i++)
                _buffer[pos + i] = (byte)RandomNumberGenerator.GetInt32(0, 255);
        }

        public void Write(Memory<byte> memory)
        {
            var pos = _position;
            Advance(memory.Length);

            memory.Span.CopyTo(_buffer.AsSpan(pos));
        }

        public void Write(byte[] buffer)
        {
            var pos = _position;
            Advance(buffer.Length);

            buffer.AsSpan().CopyTo(_buffer.AsSpan(pos));
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            var pos = _position;
            Advance(count);

            buffer.AsSpan(offset, count).CopyTo(_buffer.AsSpan(pos));
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            var pos = _position;
            Advance(buffer.Length);

            buffer.CopyTo(_buffer.AsSpan(pos));
        }

        public void Write(byte value)
        {
            var pos = _position;
            Advance(1);

            _buffer[pos] = value;
        }

        private void WriteUaligned<T>(T value, int size)
        {
            var pos = _position;
            Advance(size);

            var targetSpan = _buffer.AsSpan(pos);
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(targetSpan), value);
        }

        public void Write(short value)
        {
            WriteUaligned(value, 2);
        }

        public void Write(int value)
        {
            WriteUaligned(value, 4);
        }

        public void Write(long value)
        {
            WriteUaligned(value, 8);
        }

        public void Write(ushort value)
        {
            WriteUaligned(value, 2);
        }

        public void Write(uint value)
        {
            WriteUaligned(value, 4);
        }

        public void Write(ulong value)
        {
            WriteUaligned(value, 8);
        }

        public void Write(float value)
        {
            WriteUaligned(value, 4);
        }

        public void Write(double value)
        {
            WriteUaligned(value, 8);
        }

        public void Write(bool value)
        {
            WriteUaligned(value, 1);
        }

        public void Write(char value)
        {
            WriteUaligned(value, 2);
        }

        public void WriteUint16BigEndian(ushort value)
        {
            WriteUaligned(BinaryPrimitives.ReverseEndianness(value), 2);
        }

        public void WriteUInt24BigEndian(uint value)
        {
            var pos = _position;
            Advance(3);

            _position = pos;
            Write((byte)(value >> 16));
            Write((byte)(value >> 8));
            Write((byte)(value));
        }

        public void WriteUInt32BigEndian(uint value)
        {
            WriteUaligned(BinaryPrimitives.ReverseEndianness(value), 4);
        }

        public void WriteInt16BigEndian(short value)
        {
            WriteUaligned(BinaryPrimitives.ReverseEndianness(value), 2);
        }

        public void WriteInt24BigEndian(int value)
        {
            var pos = _position;
            Advance(3);

            _position = pos;
            Write((byte)(value >> 16));
            Write((byte)(value >> 8));
            Write((byte)(value));
        }

        public void WriteInt32BigEndian(int value)
        {
            WriteUaligned(BinaryPrimitives.ReverseEndianness(value), 4);
        }
    }
}
