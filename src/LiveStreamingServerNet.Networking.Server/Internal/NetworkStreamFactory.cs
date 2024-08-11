using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Networking.Server.Internal
{
    internal class NetworkStreamFactory : INetworkStreamFactory
    {
        private readonly ISslStreamFactory _sslStreamFactory;

        public NetworkStreamFactory(ISslStreamFactory sslStreamFactory)
        {
            _sslStreamFactory = sslStreamFactory;
        }

        public async Task<INetworkStream> CreateNetworkStreamAsync(
            uint clientId,
            ITcpClientInternal tcpClient,
            ServerEndPoint serverEndPoint,
            CancellationToken cancellationToken)
        {
            if (serverEndPoint.IsSecure)
            {
                var sslStream = await _sslStreamFactory.CreateAsync(tcpClient, cancellationToken);

                if (sslStream != null)
                    return CreateNetworkStream(clientId, sslStream);
            }

            return CreateNetworkStream(clientId, tcpClient.GetStream());
        }

        private INetworkStream CreateNetworkStream(uint clientId, Stream stream)
        {
            return new ClientNetworkStream(clientId, stream);
        }
    }
}
