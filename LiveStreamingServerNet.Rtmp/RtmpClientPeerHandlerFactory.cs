using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp
{
    internal class RtmpClientPeerHandlerFactory : IClientPeerHandlerFactory
    {
        private readonly IServiceProvider _services;

        public RtmpClientPeerHandlerFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IClientPeerHandler CreateClientPeerHandler(IClientPeerHandle clientPeer)
        {
            var handler = _services.GetRequiredService<IRtmpClientPeerHandler>();
            handler.InitializeAsync(new RtmpClientPeerContext(clientPeer));
            return handler;
        }
    }
}
