namespace LiveStreamingServerNet.Networking.Client.Contracts
{
    public interface IClient : IAsyncDisposable
    {
        IServiceProvider Services { get; }
        Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default);
    }
}
