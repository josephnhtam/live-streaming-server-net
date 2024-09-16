using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Server.Internal
{
    internal class ClientSessionFactory : ISessionFactory
    {
        private readonly IBufferSenderFactory _senderFactory;
        private readonly INetworkStreamFactory _networkStreamFactory;
        private readonly ISessionHandlerFactory _handlerFactory;
        private readonly ILogger<Session> _logger;

        public ClientSessionFactory(
            IBufferSenderFactory senderFactory,
            INetworkStreamFactory networkStreamFactory,
            ISessionHandlerFactory handlerFactory,
            ILogger<Session> logger)
        {
            _senderFactory = senderFactory;
            _networkStreamFactory = networkStreamFactory;
            _handlerFactory = handlerFactory;
            _logger = logger;
        }

        public ISession Create(uint id, ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint)
        {
            return new Session(id, tcpClient, serverEndPoint, _senderFactory, _networkStreamFactory, _handlerFactory, _logger);
        }
    }
}
