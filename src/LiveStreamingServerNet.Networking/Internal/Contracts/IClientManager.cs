using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface IClientManager
    {
        IReadOnlyList<IClientHandle> GetClientHandles();
        IClientHandle? GetClient(uint clientId);
        Task AcceptClientAsync(ITcpListenerInternal tcpListener, ServerEndPoint serverEndPoint, CancellationToken cancellationToken);
        Task WaitUntilAllClientTasksCompleteAsync(CancellationToken cancellationToken = default);
    }
}
