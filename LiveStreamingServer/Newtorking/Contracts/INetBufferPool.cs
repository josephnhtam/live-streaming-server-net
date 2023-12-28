namespace LiveStreamingServer.Newtorking.Contracts
{
    public interface INetBufferPool : IDisposable
    {
        INetBuffer Obtain();
    }
}
