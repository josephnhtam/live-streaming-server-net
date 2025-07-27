using LiveStreamingServerNet.Networking.Client.Internal.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;

namespace LiveStreamingServerNet.Networking.Client.Internal
{
    internal class NetworkStreamFactory : INetworkStreamFactory
    {
        private readonly ISslStreamFactory _sslStreamFactory;

        public NetworkStreamFactory(ISslStreamFactory sslStreamFactory)
        {
            _sslStreamFactory = sslStreamFactory;
        }

        public async Task<INetworkStream> CreateNetworkStreamAsync(
            uint id, ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint, CancellationToken cancellationToken)
        {
            if (serverEndPoint.IsSecure)
            {
                var sslStream = await _sslStreamFactory.CreateAsync(tcpClient, cancellationToken).ConfigureAwait(false);

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
