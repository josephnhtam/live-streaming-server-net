using System.Security.Cryptography;

namespace LiveStreamingServerNet.Newtorking
{
    public partial class NetBufferBase
    {
        public void WriteRandomBytes(int count)
        {
            Size = Math.Max(Size, Position + count);
            var writer = GetWriter();
            for (int i = 0; i < count; i++)
                writer.Write((byte)RandomNumberGenerator.GetInt32(0, 255));
            writer.Flush();
            RefreshSize();
        }

        public void Write(Memory<byte> memory)
        {
            var writer = GetWriter();
            writer.Write(memory.Span);
            writer.Flush();
            RefreshSize();
        }

        public void Write(byte[] buffer)
        {
            var writer = GetWriter();
            writer.Write(buffer, 0, buffer.Length);
            writer.Flush();
            RefreshSize();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            var writer = GetWriter();
            writer.Write(buffer, offset, count);
            writer.Flush();
            RefreshSize();
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            var writer = GetWriter();
            writer.Write(buffer);
            writer.Flush();
            RefreshSize();
        }

        public void Write(byte value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(short value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(int value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(long value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(ushort value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(uint value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(ulong value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(float value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(double value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(string value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(bool value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void Write(char value)
        {
            var writer = GetWriter();
            writer.Write(value);
            writer.Flush();
            RefreshSize();
        }

        public void WriteUint16BigEndian(ushort value)
        {
            var writer = GetWriter();
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
            writer.Flush();
            RefreshSize();
        }

        public void WriteUInt24BigEndian(uint value)
        {
            var writer = GetWriter();
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
            RefreshSize();
        }

        public void WriteUInt32BigEndian(uint value)
        {
            var writer = GetWriter();
            writer.Write((byte)(value >> 24));
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
            RefreshSize();
        }

        public void WriteInt16BigEndian(short value)
        {
            var writer = GetWriter();
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
            RefreshSize();
        }

        public void WriteInt24BigEndian(int value)
        {
            var writer = GetWriter();
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
            RefreshSize();
        }

        public void WriteInt32BigEndian(int value)
        {
            var writer = GetWriter();
            writer.Write((byte)(value >> 24));
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
            RefreshSize();
        }
    }
}
