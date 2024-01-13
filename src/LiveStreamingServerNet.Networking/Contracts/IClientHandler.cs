namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IClientHandler : IAsyncDisposable
    {
        Task<bool> HandleClientLoopAsync(ReadOnlyStream networkStream, CancellationToken cancellationToken);
    }
}
