using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public partial class DataBuffer : IDataBuffer
    {
        private readonly IBufferPool? _bufferPool;
        private byte[] _buffer;
        private int _position;
        private int _size;
        private bool _isDisposed;

        public int Position
        {
            get => _position;
            set
            {
                EnsureCapacity(value);
                _position = value;
            }
        }

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

        public byte[] UnderlyingBuffer => _buffer;

        public DataBuffer() : this(1024) { }

        public DataBuffer(int initialCapacity) : this(null, initialCapacity) { }

        public DataBuffer(IBufferPool? bufferPool, int initialCapacity)
        {
            _bufferPool = bufferPool;
            _buffer = _bufferPool?.Rent(initialCapacity) ?? ArrayPool<byte>.Shared.Rent(initialCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int capacity)
        {
            if (capacity < Capacity)
                return;

            byte[] buffer;

            if (_bufferPool != null)
            {
                buffer = _bufferPool.Rent(capacity);
                _buffer.AsSpan().CopyTo(buffer);
                _bufferPool.Return(_buffer);
            }
            else
            {
                buffer = ArrayPool<byte>.Shared.Rent(capacity);
                _buffer.AsSpan().CopyTo(buffer);
                ArrayPool<byte>.Shared.Return(_buffer);
            }

            _buffer = buffer;
        }

        public IDataBuffer MoveTo(int position)
        {
            Position = position;
            return this;
        }

        public void Advance(int count)
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

        public void Flush(IDataBuffer output)
        {
            output.Write(_buffer, 0, Size);
            Reset();
        }

        public void Flush(Stream output)
        {
            output.Write(_buffer, 0, Size);
            Reset();
        }

        public void CopyAllTo(IDataBuffer targetBuffer)
        {
            targetBuffer.Write(_buffer, 0, Size);
        }

        public void ReadAndWriteTo(IDataBuffer targetBuffer, int bytesCount)
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

        public ValueTask FromStreamData(IStreamReader streamReader, int bytesCount, CancellationToken cancellationToken = default)
        {
            _position = 0;
            Size = bytesCount;
            return streamReader.ReadExactlyAsync(_buffer, 0, bytesCount, cancellationToken);
        }

        public ValueTask AppendStreamData(IStreamReader streamReader, int bytesCount, CancellationToken cancellationToken = default)
        {
            var pos = _position;
            Advance(bytesCount);
            return streamReader.ReadExactlyAsync(_buffer, pos, bytesCount, cancellationToken);
        }

        public IRentedBuffer ToRentedBuffer(int offset, int size, int initialClaim = 1)
        {
            Debug.Assert(offset + size <= _size);

            var originalPosition = _position;

            try
            {
                var rentedBuffer = new RentedBuffer(_bufferPool, size, initialClaim);

                _position = offset;
                ReadBytes(rentedBuffer.Buffer, offset, size);

                return rentedBuffer;
            }
            finally
            {
                _position = originalPosition;
            }
        }

        public IRentedBuffer ToRentedBuffer(int initialClaim = 1)
        {
            return ToRentedBuffer(0, _size, initialClaim);
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_bufferPool != null)
                _bufferPool.Return(_buffer);
            else
                ArrayPool<byte>.Shared.Return(_buffer);

            _buffer = null!;
        }
    }
}
