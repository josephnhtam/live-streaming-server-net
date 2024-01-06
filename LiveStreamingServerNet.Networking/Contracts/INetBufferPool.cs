namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface INetBufferPool : IDisposable
    {
        INetBuffer Obtain();
    }
}
