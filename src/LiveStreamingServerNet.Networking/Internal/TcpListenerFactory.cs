using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class TcpListenerFactory : ITcpListenerFactory
    {
        private readonly NetworkConfiguration _config;

        public TcpListenerFactory(IOptions<NetworkConfiguration> config)
        {
            _config = config.Value;
        }

        public ITcpListenerInternal Create(IPEndPoint endpoint)
        {
            return new TcpListenerWrapper(new TcpListener(endpoint), _config);
        }
    }
}
