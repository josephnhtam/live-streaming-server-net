namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface INetBufferPool
    {
        INetBuffer Obtain();
        void Recycle(INetBuffer netBuffer);
    }
}
