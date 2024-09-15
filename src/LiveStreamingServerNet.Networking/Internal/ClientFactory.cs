using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class ClientFactory : IClientFactory
    {
        private readonly IClientBufferSenderFactory _senderFactory;
        private readonly INetworkStreamFactory _networkStreamFactory;
        private readonly IClientHandlerFactory _clientHandlerFactory;
        private readonly ILogger<Client> _logger;

        public ClientFactory(
            IClientBufferSenderFactory senderFactory,
            INetworkStreamFactory networkStreamFactory,
            IClientHandlerFactory clientHandlerFactory,
            ILogger<Client> logger)
        {
            _senderFactory = senderFactory;
            _networkStreamFactory = networkStreamFactory;
            _clientHandlerFactory = clientHandlerFactory;
            _logger = logger;
        }

        public IClient Create(uint clientId, ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint)
        {
            return new Client(clientId, tcpClient, serverEndPoint, _senderFactory, _networkStreamFactory, _clientHandlerFactory, _logger);
        }
    }
}
