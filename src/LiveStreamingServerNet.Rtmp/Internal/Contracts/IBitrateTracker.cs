namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    public interface IBitrateTracker
    {
        void AddBytes(int bytes);
        int GetBitrateKbps();
        void Reset();
    }
}
