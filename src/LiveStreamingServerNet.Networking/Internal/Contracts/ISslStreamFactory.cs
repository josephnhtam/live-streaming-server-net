using System.Net.Security;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface ISslStreamFactory
    {
        Task<SslStream?> CreateAsync(ITcpClientInternal tcpClient, CancellationToken cancellationToken);
    }
}
