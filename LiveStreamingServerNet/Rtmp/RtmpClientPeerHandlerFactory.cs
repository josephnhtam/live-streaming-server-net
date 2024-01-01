using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp
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
