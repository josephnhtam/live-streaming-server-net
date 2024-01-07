namespace LiveStreamingServerNet.Utilities.Contracts
{
    public interface IRentedBuffer
    {
        byte[] Buffer { get; }
        int Claimed { get; }

        void Claim();
        void Unclaim();
    }
}