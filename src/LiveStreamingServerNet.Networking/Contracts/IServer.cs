namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IServer : IServerHandle
    {
        IServiceProvider Services { get; }
        Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default);
        Task RunAsync(IList<ServerEndPoint> serverEndPoints, CancellationToken cancellationToken = default);
    }

    public interface IServerHandle
    {
        bool IsStarted { get; }
        IReadOnlyList<IClientHandle> Clients { get; }
        IClientHandle? GetClient(uint clientId);
    }
}
