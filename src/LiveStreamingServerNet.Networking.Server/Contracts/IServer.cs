using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Server.Contracts
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
        IReadOnlyList<ISessionHandle> Clients { get; }
        ISessionHandle? GetClient(uint clientId);
    }
}
