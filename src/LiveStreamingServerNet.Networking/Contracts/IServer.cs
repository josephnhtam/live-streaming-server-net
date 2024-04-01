namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IServer : IServerHandle
    {
        IServiceProvider Services { get; }
        Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default);
        Task RunAsync(IReadOnlyList<ServerEndPoint> serverEndPoints, CancellationToken cancellationToken = default);
    }

    public interface IServerHandle
    {
        bool IsStarted { get; }
        IReadOnlyList<ServerEndPoint>? EndPoints { get; }
        IReadOnlyList<IClientHandle> Clients { get; }
        IClientHandle? GetClient(uint clientId);
    }
}
