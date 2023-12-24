namespace LiveStreamingServer.Newtorking.Contracts
{
    public interface INetBufferPool
    {
        INetBuffer ObtainNetBuffer();
        void RecycleNetBuffer(INetBuffer netBuffer);
    }
}
