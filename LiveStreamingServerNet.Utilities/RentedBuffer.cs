using LiveStreamingServerNet.Utilities.Contracts;
using System.Buffers;

namespace LiveStreamingServerNet.Utilities
{
    public class RentedBuffer : IRentedBuffer
    {
        public byte[] Buffer => _buffer;
        public int Claimed => _claimed;

        private byte[] _buffer;
        private int _claimed;

        public RentedBuffer(int size, int initialClaim = 1)
        {
            if (initialClaim <= 0)
                throw new ArgumentOutOfRangeException(nameof(initialClaim));

            _buffer = ArrayPool<byte>.Shared.Rent(size);
            _claimed = initialClaim;
        }

        public void Claim()
        {
            Interlocked.Increment(ref _claimed);
        }

        public void Unclaim()
        {
            byte[] bytes;

            if (Interlocked.Decrement(ref _claimed) <= 0 && (bytes = Interlocked.Exchange(ref _buffer, null!)) != null)
                ArrayPool<byte>.Shared.Return(_buffer);
        }
    }
}
