using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class ClientFactory : IClientFactory
    {
        private readonly IClientBufferSenderFactory _senderFactory;
        private readonly INetworkStreamFactory _networkStreamFactory;
        private readonly ILogger<Client> _logger;

        public ClientFactory(
            IClientBufferSenderFactory senderFactory,
            INetworkStreamFactory networkStreamFactory,
            ILogger<Client> logger)
        {
            _senderFactory = senderFactory;
            _networkStreamFactory = networkStreamFactory;
            _logger = logger;
        }

        public IClient Create(uint clientId, ITcpClientInternal tcpClient)
        {
            var bufferSender = _senderFactory.Create(clientId);
            return new Client(clientId, tcpClient, bufferSender, _networkStreamFactory, _logger);
        }
    }
}
