namespace LiveStreamingServerNet.Utilities.Buffers.Contracts
{
    public interface IBufferCache<TBufferInfo> : IDisposable
    {
        int Size { get; }

        void Reset();
        void Write(TBufferInfo info, IRentedBuffer buffer);
        void Write(TBufferInfo info, ReadOnlySpan<byte> buffer);
        IList<(TBufferInfo Info, IRentedBuffer Buffer)> GetBuffers(int initialClaim = 1);
    }
}