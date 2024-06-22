namespace LiveStreamingServerNet.Utilities.Buffers.Contracts
{
    public interface IBufferPool
    {
        byte[] Rent(int minimumLength);
        void Return(byte[] buffer);
    }
}
