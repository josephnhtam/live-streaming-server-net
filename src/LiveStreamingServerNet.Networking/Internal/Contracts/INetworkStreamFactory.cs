using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface INetworkStreamFactory
    {
        Task<INetworkStream> CreateNetworkStreamAsync(ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint, CancellationToken cancellationToken);
    }
}
