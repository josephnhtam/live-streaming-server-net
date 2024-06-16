using LiveStreamingServerNet.Networking.Contracts;
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

        public async Task<INetworkStream> CreateNetworkStreamAsync(
            ITcpClientInternal tcpClient,
            ServerEndPoint serverEndPoint,
            CancellationToken cancellationToken)
        {
            if (serverEndPoint.IsSecure)
            {
                var sslStream = await _sslStreamFactory.CreateAsync(tcpClient, cancellationToken);

                if (sslStream != null)
                    return CreateNetworkStream(sslStream);
            }

            return CreateNetworkStream(tcpClient.GetStream());
        }

        private INetworkStream CreateNetworkStream(Stream stream)
        {
            return new NetworkStream(stream);
        }
    }
}
