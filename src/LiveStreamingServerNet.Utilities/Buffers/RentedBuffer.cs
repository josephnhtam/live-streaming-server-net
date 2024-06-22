using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Buffers;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public sealed class RentedBuffer : IRentedBuffer
    {
        private readonly IBufferPool? _bufferPool;
        private byte[] _buffer;
        private int _claimed;

        public byte[] Buffer => _buffer;
        public int Claimed => _claimed;
        public int Size { get; }

        public RentedBuffer(int size, int initialClaim = 1) : this(null, size, initialClaim) { }

        public RentedBuffer(IBufferPool? bufferPool, int size, int initialClaim = 1)
        {
            if (initialClaim <= 0)
                throw new ArgumentOutOfRangeException(nameof(initialClaim));

            _bufferPool = bufferPool;
            _buffer = _bufferPool?.Rent(size) ?? ArrayPool<byte>.Shared.Rent(size);
            _claimed = initialClaim;
            Size = size;
        }

        public void Claim(int count = 1)
        {
            Interlocked.Add(ref _claimed, count);
        }

        public void Unclaim(int count = 1)
        {
            byte[] bytes;

            if (Interlocked.Add(ref _claimed, -count) <= 0 && (bytes = Interlocked.Exchange(ref _buffer, null!)) != null)
            {
                if (_bufferPool != null)
                    _bufferPool.Return(bytes);
                else
                    ArrayPool<byte>.Shared.Return(bytes);
            }
        }

        public ReadOnlySpan<byte> AsSpan()
        {
            return _buffer.AsSpan(0, Size);
        }

        public IRentedBuffer Clone(int initialClaim = 1)
        {
            var clone = new RentedBuffer(_bufferPool, Size, initialClaim);
            _buffer.AsSpan(0, Size).CopyTo(clone.Buffer);
            return clone;
        }
    }
}
