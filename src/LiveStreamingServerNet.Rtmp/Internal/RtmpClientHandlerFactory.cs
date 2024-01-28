using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientHandlerFactory : IClientHandlerFactory
    {
        private readonly IServiceProvider _services;

        public RtmpClientHandlerFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IClientHandler CreateClientHandler()
        {
            return _services.GetRequiredService<IRtmpClientHandler>();
        }
    }
}
