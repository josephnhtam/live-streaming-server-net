using LiveStreamingServerNet.Networking.Contracts;
using System.Buffers;

namespace LiveStreamingServerNet.Networking
{
    public partial class NetBuffer : INetBuffer
    {
        private bool _isDisposed;

        private byte[] _buffer;
        public byte[] UnderlyingBuffer => _buffer;

        private int _position;
        public int Position
        {
            get => _position;
            set
            {
                EnsureCapacity(value);
                _position = value;
            }
        }

        private int _size;
        public int Size
        {
            get => _size;
            set
            {
                EnsureCapacity(value);
                _size = value;
            }
        }

        public int Capacity => _buffer.Length;

        public NetBuffer()
        {
            _buffer = ArrayPool<byte>.Shared.Rent(128);
        }

        public NetBuffer(int initialCapacity)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity < Capacity)
                return;

            var newCapacity = Math.Max(capacity, Capacity * 2);

            var buffer = ArrayPool<byte>.Shared.Rent(newCapacity);
            _buffer.AsSpan().CopyTo(buffer);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = buffer;
        }

        public INetBuffer MoveTo(int position)
        {
            Position = position;
            return this;
        }

        private void Advance(int count)
        {
            Position += count;
            _size = Math.Max(_size, Position);
        }

        public void Reset()
        {
            _position = 0;
            _size = 0;
        }

        public async Task FlushAsync(Stream output)
        {
            await output.WriteAsync(_buffer, 0, Size);
            Reset();
        }

        public void Flush(INetBuffer output)
        {
            output.Write(_buffer, 0, Size);
            Reset();
        }

        public void Flush(Stream output)
        {
            output.Write(_buffer, 0, Size);
            Reset();
        }

        public void CopyAllTo(INetBuffer targetBuffer)
        {
            targetBuffer.Write(_buffer, 0, Size);
        }

        public void ReadAndWriteTo(INetBuffer targetBuffer, int bytesCount)
        {
            if (_position + bytesCount > _size)
                throw new ArgumentOutOfRangeException(nameof(bytesCount));

            targetBuffer.Write(_buffer, _position, bytesCount);
            _position += bytesCount;
        }

        public ValueTask FromStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default)
        {
            _position = 0;
            Size = bytesCount;
            return stream.ReadExactlyAsync(_buffer, 0, bytesCount, cancellationToken);
        }

        public ValueTask AppendStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default)
        {
            var pos = _position;
            Advance(bytesCount);
            return stream.ReadExactlyAsync(_buffer, pos, bytesCount, cancellationToken);
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null!;
        }
    }
}
