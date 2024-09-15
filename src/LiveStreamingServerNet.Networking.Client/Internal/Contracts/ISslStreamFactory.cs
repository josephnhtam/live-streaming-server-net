using LiveStreamingServerNet.Networking.Internal.Contracts;
using System.Net.Security;

namespace LiveStreamingServerNet.Networking.Client.Internal.Contracts
{
    internal interface ISslStreamFactory
    {
        Task<SslStream> CreateAsync(ITcpClientInternal tcpClient, CancellationToken cancellationToken);
    }
}
