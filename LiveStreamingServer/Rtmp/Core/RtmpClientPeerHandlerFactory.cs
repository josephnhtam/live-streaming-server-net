using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerHandlerFactory : IClientPeerHandlerFactory
    {
        private readonly IServiceProvider _services;

        public RtmpClientPeerHandlerFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IClientPeerHandler CreateClientPeerHandler(IClientPeerHandle clientPeer)
        {
            var handler = _services.GetRequiredService<IRtmpClientPeerHandler>();
            handler.Initialize(new RtmpClientPeerContext(clientPeer));
            return handler;
        }
    }
}
