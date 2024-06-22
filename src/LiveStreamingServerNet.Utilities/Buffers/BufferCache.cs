using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Buffers;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public sealed class BufferCache<TBufferInfo> : IBufferCache<TBufferInfo>
    {
        private readonly IBufferPool? _bufferPool;
        private readonly List<BufferInfoWithSize> _bufferInfos;
        private readonly object _syncLock;

        private byte[] _buffer;
        private int _size;

        public int Size => _size;

        public BufferCache(int initialCapacity) : this(null, initialCapacity) { }

        public BufferCache(IBufferPool? bufferPool, int initialCapacity)
        {
            _bufferPool = bufferPool;
            _buffer = _bufferPool?.Rent(initialCapacity) ?? ArrayPool<byte>.Shared.Rent(initialCapacity);
            _bufferInfos = new List<BufferInfoWithSize>();
            _syncLock = new object();
        }

        public void Reset()
        {
            lock (_syncLock)
            {
                _size = 0;
                _bufferInfos.Clear();
            }
        }

        public void Write(TBufferInfo info, IRentedBuffer buffer)
        {
            Write(info, buffer.Buffer.AsSpan(0, buffer.Size));
        }

        public void Write(TBufferInfo info, ReadOnlySpan<byte> buffer)
        {
            lock (_syncLock)
            {
                EnsureCapacity(_size + buffer.Length);

                buffer.CopyTo(_buffer.AsSpan(_size));
                _size += buffer.Length;
                _bufferInfos.Add(new BufferInfoWithSize(info, buffer.Length));
            }
        }

        public IList<(TBufferInfo Info, IRentedBuffer Buffer)> GetBuffers(int initialClaim = 1)
        {
            lock (_syncLock)
            {
                var buffers = new List<(TBufferInfo, IRentedBuffer)>(_bufferInfos.Count);
                int pos = 0;

                foreach (var bufferInfo in _bufferInfos)
                {
                    var rentedBuffer = new RentedBuffer(_bufferPool, bufferInfo.Size, initialClaim);
                    _buffer.AsSpan(pos, bufferInfo.Size).CopyTo(rentedBuffer.Buffer);

                    buffers.Add((bufferInfo.Info, rentedBuffer));
                    pos += bufferInfo.Size;
                }

                return buffers;
            }
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity < _buffer.Length)
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

        public void Dispose()
        {
            if (_bufferPool != null)
                _bufferPool.Return(_buffer);
            else
                ArrayPool<byte>.Shared.Return(_buffer);
        }

        private record struct BufferInfoWithSize(TBufferInfo Info, int Size);
    }
}
