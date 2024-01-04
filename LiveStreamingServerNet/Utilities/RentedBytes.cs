using System.Buffers;

namespace LiveStreamingServerNet.Utilities
{
    public class RentedBytes
    {
        public byte[] Bytes { get; private set; }
        public int Claimed { get; private set; }

        public RentedBytes(int size, int claim = 1)
        {
            Bytes = ArrayPool<byte>.Shared.Rent(size);
            Claimed = claim;
        }

        public void Claim()
        {
            lock (Bytes)
            {
                Claimed++;
            }
        }

        public void Unclaim()
        {
            lock (Bytes)
            {
                if (--Claimed <= 0)
                {
                    ArrayPool<byte>.Shared.Return(Bytes);
                }
            }
        }
    }
}
