using LiveStreamingServerNet.Networking.Installer.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface INetworkStreamFactory
    {
        Task<Stream> CreateNetworkStreamAsync(ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint, CancellationToken cancellationToken);
    }
}
