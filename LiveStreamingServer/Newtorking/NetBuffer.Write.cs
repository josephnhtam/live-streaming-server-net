using System.Security.Cryptography;

namespace LiveStreamingServer.Newtorking
{
    public partial class NetBuffer
    {
        private void RefreshSize()
        {
            Size = Math.Max(Size, Position);
        }

        public void WriteRandomBytes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _stream.WriteByte((byte)RandomNumberGenerator.GetInt32(0, 255));
            }
        }

        public void Write(Memory<byte> memory)
        {
            _stream.Write(memory.Span);
            RefreshSize();
        }

        public void Write(byte[] buffer)
        {
            _stream.Write(buffer, 0, buffer.Length);
            RefreshSize();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
            RefreshSize();
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            _stream.Write(buffer);
            RefreshSize();
        }

        public void Write(byte value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(short value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(int value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(long value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(ushort value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(uint value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(ulong value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(float value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(double value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(string value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(bool value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }

        public void Write(char value)
        {
            _writer.Write(value);
            _writer.Flush();
            RefreshSize();
        }
    }
}
