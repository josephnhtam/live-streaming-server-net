using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking
{
    internal class ClientFactory : IClientFactory
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly IOptions<SecurityConfiguration> _config;
        private readonly ILogger<Client> _logger;

        public ClientFactory(INetBufferPool netBufferPool, IOptions<SecurityConfiguration> config, ILogger<Client> logger)
        {
            _netBufferPool = netBufferPool;
            _config = config;
            _logger = logger;
        }

        public IClient Create(uint clientId, TcpClient tcpClient)
        {
            return new Client(clientId, tcpClient, _netBufferPool, _config, _logger);
        }
    }
}
