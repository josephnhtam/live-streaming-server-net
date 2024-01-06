namespace LiveStreamingServerNet.Utilities.Contracts
{
    public interface IRentedBuffer
    {
        byte[] Bytes { get; }
        int Claimed { get; }

        void Claim();
        void Unclaim();
    }
}