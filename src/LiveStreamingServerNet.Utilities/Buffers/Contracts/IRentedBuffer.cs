namespace LiveStreamingServerNet.Utilities.Buffers.Contracts
{
    public interface IRentedBuffer
    {
        byte[] Buffer { get; }
        int Claimed { get; }
        int Size { get; }

        void Claim(int count = 1);
        void Unclaim(int count = 1);

        ReadOnlySpan<byte> AsSpan();
        IRentedBuffer Clone(int initialClaim = 1);
    }
}