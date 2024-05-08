using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class NetworkStreamFactory : INetworkStreamFactory
    {
        private readonly ISslStreamFactory _sslStreamFactory;
        private readonly NetworkConfiguration _config;

        public NetworkStreamFactory(ISslStreamFactory sslStreamFactory, IOptions<NetworkConfiguration> config)
        {
            _sslStreamFactory = sslStreamFactory;
            _config = config.Value;
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
            if (_config.FlushingInterval > TimeSpan.Zero)
                return new BufferedNetworkStream(stream, _config);

            return new NetworkStream(stream);
        }
    }
}
