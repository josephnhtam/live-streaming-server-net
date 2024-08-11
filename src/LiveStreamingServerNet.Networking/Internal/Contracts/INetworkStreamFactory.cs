using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface INetworkStreamFactory
    {
        Task<INetworkStream> CreateNetworkStreamAsync(uint id, ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint, CancellationToken cancellationToken);
    }
}
