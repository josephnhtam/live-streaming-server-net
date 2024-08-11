using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Server.Internal.Contracts
{
    internal interface IClientSessionManager
    {
        IReadOnlyList<ISessionHandle> GetClients();
        ISessionHandle? GetClient(uint clientId);
        Task AcceptClientAsync(ITcpListenerInternal tcpListener, ServerEndPoint serverEndPoint, CancellationToken cancellationToken);
        Task WaitUntilAllClientTasksCompleteAsync(CancellationToken cancellationToken = default);
    }
}
