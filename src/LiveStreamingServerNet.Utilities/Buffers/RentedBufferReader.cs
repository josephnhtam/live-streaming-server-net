using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Internal;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public sealed class RentedBufferReader : IDataBufferReader
    {
        private readonly IRentedBuffer _rentedBuffer;
        private readonly byte[] _buffer;

        private int _position;
        private int _isDisposed;

        public RentedBufferReader(IRentedBuffer rentedBuffer)
        {
            rentedBuffer.Claim();

            _rentedBuffer = rentedBuffer;
            _buffer = rentedBuffer.Buffer;
            Size = rentedBuffer.Size;
        }

        public int Size { get; }

        public int Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > Size)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _position = value;
            }
        }

        public bool ReadBoolean()
            => DataBufferRead.ReadBoolean(_buffer, 0, Size, ref _position);

        public byte ReadByte()
            => DataBufferRead.ReadByte(_buffer, 0, Size, ref _position);

        public void ReadBytes(byte[] buffer, int index, int count)
            => DataBufferRead.ReadBytes(_buffer, 0, Size, ref _position, buffer, index, count);

        public void ReadBytes(Span<byte> buffer)
            => DataBufferRead.ReadBytes(_buffer, 0, Size, ref _position, buffer);

        public byte[] ReadBytes(int count)
            => DataBufferRead.ReadBytes(_buffer, 0, Size, ref _position, count);

        public char ReadChar()
            => DataBufferRead.ReadChar(_buffer, 0, Size, ref _position);

        public double ReadDouble()
            => DataBufferRead.ReadDouble(_buffer, 0, Size, ref _position);

        public short ReadInt16()
            => DataBufferRead.ReadInt16(_buffer, 0, Size, ref _position);

        public int ReadInt32()
            => DataBufferRead.ReadInt32(_buffer, 0, Size, ref _position);

        public long ReadInt64()
            => DataBufferRead.ReadInt64(_buffer, 0, Size, ref _position);

        public float ReadSingle()
            => DataBufferRead.ReadSingle(_buffer, 0, Size, ref _position);

        public ushort ReadUInt16()
            => DataBufferRead.ReadUInt16(_buffer, 0, Size, ref _position);

        public uint ReadUInt32()
            => DataBufferRead.ReadUInt32(_buffer, 0, Size, ref _position);

        public ulong ReadUInt64()
            => DataBufferRead.ReadUInt64(_buffer, 0, Size, ref _position);

        public ushort ReadUInt16BigEndian()
            => DataBufferRead.ReadUInt16BigEndian(_buffer, 0, Size, ref _position);

        public uint ReadUInt24BigEndian()
            => DataBufferRead.ReadUInt24BigEndian(_buffer, 0, Size, ref _position);

        public uint ReadUInt32BigEndian()
            => DataBufferRead.ReadUInt32BigEndian(_buffer, 0, Size, ref _position);

        public ulong ReadUInt64BigEndian()
            => DataBufferRead.ReadUInt64BigEndian(_buffer, 0, Size, ref _position);

        public short ReadInt16BigEndian()
            => DataBufferRead.ReadInt16BigEndian(_buffer, 0, Size, ref _position);

        public int ReadInt24BigEndian()
            => DataBufferRead.ReadInt24BigEndian(_buffer, 0, Size, ref _position);

        public int ReadInt32BigEndian()
            => DataBufferRead.ReadInt32BigEndian(_buffer, 0, Size, ref _position);

        public long ReadInt64BigEndian()
            => DataBufferRead.ReadInt64BigEndian(_buffer, 0, Size, ref _position);

        public string ReadUtf8String(int length)
            => DataBufferRead.ReadUtf8String(_buffer, 0, Size, ref _position, length);

        public ReadOnlySpan<byte> AsReadOnlySpan()
            => DataBufferView.AsReadOnlySpan(_buffer, 0, Size);

        public ReadOnlySpan<byte> AsReadOnlySpan(int offset)
            => DataBufferView.AsReadOnlySpan(_buffer, 0, Size, offset);

        public ReadOnlySpan<byte> AsReadOnlySpan(int offset, int length)
            => DataBufferView.AsReadOnlySpan(_buffer, 0, Size, offset, length);

        public ReadOnlyMemory<byte> AsReadOnlyMemory()
            => DataBufferView.AsReadOnlyMemory(_buffer, 0, Size);

        public ReadOnlyMemory<byte> AsReadOnlyMemory(int offset)
            => DataBufferView.AsReadOnlyMemory(_buffer, 0, Size, offset);

        public ReadOnlyMemory<byte> AsReadOnlyMemory(int offset, int length)
            => DataBufferView.AsReadOnlyMemory(_buffer, 0, Size, offset, length);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 1)
                return;

            _rentedBuffer.Unclaim();
        }
    }
}
