using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class NetworkStreamFactory : INetworkStreamFactory
    {
        private readonly ISslStreamFactory _sslStreamFactory;

        public NetworkStreamFactory(ISslStreamFactory sslStreamFactory)
        {
            _sslStreamFactory = sslStreamFactory;
        }

        public async Task<Stream> CreateNetworkStreamAsync(
            ITcpClientInternal tcpClient,
            ServerEndPoint serverEndPoint,
            CancellationToken cancellationToken)
        {
            if (serverEndPoint.IsSecure)
            {
                var sslStream = await _sslStreamFactory.CreateAsync(tcpClient, cancellationToken);

                if (sslStream != null)
                    return sslStream;
            }

            return tcpClient.GetStream();
        }
    }
}
