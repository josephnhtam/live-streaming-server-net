namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface INetBufferPool : IDisposable
    {
        INetBuffer Obtain();
    }
}
