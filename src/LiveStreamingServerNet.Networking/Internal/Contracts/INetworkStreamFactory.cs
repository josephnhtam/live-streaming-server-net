using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Installer.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface INetworkStreamFactory
    {
        Task<INetworkStream> CreateNetworkStreamAsync(ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint, CancellationToken cancellationToken);
    }
}
