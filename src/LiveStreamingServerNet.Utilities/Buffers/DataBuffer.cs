using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public partial class DataBuffer : IDataBuffer
    {
        private readonly IBufferPool? _bufferPool;

        private byte[] _buffer;
        private int _startIndex;
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
            if ((_startIndex + capacity) < Capacity)
                return;

            if (capacity < Capacity)
            {
                _buffer.AsSpan(_startIndex, _size).CopyTo(_buffer);
                _startIndex = 0;
                return;
            }

            byte[] buffer;

            if (_bufferPool != null)
            {
                buffer = _bufferPool.Rent(capacity);
                _buffer.AsSpan(_startIndex, _size).CopyTo(buffer);
                _bufferPool.Return(_buffer);
            }
            else
            {
                buffer = ArrayPool<byte>.Shared.Rent(capacity);
                _buffer.AsSpan(_startIndex, _size).CopyTo(buffer);
                ArrayPool<byte>.Shared.Return(_buffer);
            }

            _startIndex = 0;
            _buffer = buffer;
        }

        public void TrimStart(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                return;

            if (count >= _size)
            {
                Reset();
                return;
            }

            _startIndex += count;
            _size -= count;
            _position -= count;
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
            _startIndex = 0;
            _position = 0;
            _size = 0;
        }

        public async Task FlushAsync(Stream output)
        {
            await output.WriteAsync(_buffer, _startIndex, _size);
            Reset();
        }

        public void Flush(IDataBuffer output)
        {
            output.Write(_buffer, _startIndex, _size);
            Reset();
        }

        public void Flush(Stream output)
        {
            output.Write(_buffer.AsSpan(_startIndex, _size));
            Reset();
        }

        public void CopyAllTo(IDataBuffer targetBuffer)
        {
            targetBuffer.Write(_buffer, _startIndex, _size);
        }

        public void ReadAndWriteTo(IDataBuffer targetBuffer, int bytesCount)
        {
            if (_position + bytesCount > _size)
                throw new ArgumentOutOfRangeException(nameof(bytesCount));

            targetBuffer.Write(_buffer, _startIndex + _position, bytesCount);
            _position += bytesCount;
        }

        public ValueTask FromStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default)
        {
            _startIndex = 0;
            _position = 0;
            Size = bytesCount;
            return stream.ReadExactlyAsync(_buffer, 0, bytesCount, cancellationToken);
        }

        public ValueTask AppendStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default)
        {
            var pos = _position;
            Advance(bytesCount);
            return stream.ReadExactlyAsync(_buffer, _startIndex + pos, bytesCount, cancellationToken);
        }

        public ValueTask FromStreamData(IStreamReader streamReader, int bytesCount, CancellationToken cancellationToken = default)
        {
            _startIndex = 0;
            _position = 0;
            Size = bytesCount;
            return streamReader.ReadExactlyAsync(_buffer, 0, bytesCount, cancellationToken);
        }

        public ValueTask AppendStreamData(IStreamReader streamReader, int bytesCount, CancellationToken cancellationToken = default)
        {
            var pos = _position;
            Advance(bytesCount);
            return streamReader.ReadExactlyAsync(_buffer, _startIndex + pos, bytesCount, cancellationToken);
        }

        public IRentedBuffer ToRentedBuffer(int offset, int size, int initialClaim = 1)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (offset + size > _size)
                throw new ArgumentOutOfRangeException(nameof(size));

            var rentedBuffer = new RentedBuffer(_bufferPool, size, initialClaim);
            _buffer.AsSpan(_startIndex + offset, size).CopyTo(rentedBuffer.Buffer);

            return rentedBuffer;
        }

        public IRentedBuffer ToRentedBuffer(int initialClaim = 1)
        {
            return ToRentedBuffer(0, _size, initialClaim);
        }

        public Span<byte> AsSpan()
        {
            return _buffer.AsSpan(_startIndex, _size);
        }

        public Span<byte> AsSpan(int offset)
        {
            return _buffer.AsSpan(_startIndex + offset, _size);
        }

        public Span<byte> AsSpan(int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (offset + length > _size)
                throw new ArgumentOutOfRangeException(nameof(length));

            return _buffer.AsSpan(_startIndex + offset, length);
        }

        public Memory<byte> AsMemory()
        {
            return _buffer.AsMemory(_startIndex, _size);
        }

        public Memory<byte> AsMemory(int offset)
        {
            return _buffer.AsMemory(_startIndex + offset, _size);
        }

        public Memory<byte> AsMemory(int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (offset + length > _size)
                throw new ArgumentOutOfRangeException(nameof(length));

            return _buffer.AsMemory(_startIndex + offset, length);
        }

        public ArraySegment<byte> AsSegment()
        {
            return new ArraySegment<byte>(_buffer, _startIndex, _size);
        }

        public ArraySegment<byte> AsSegment(int offset)
        {
            return new ArraySegment<byte>(_buffer, _startIndex + offset, _size);
        }

        public ArraySegment<byte> AsSegment(int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (offset + length > _size)
                throw new ArgumentOutOfRangeException(nameof(length));

            return new ArraySegment<byte>(_buffer, _startIndex + offset, length);
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
