using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Newtorking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Networking
{
    public static class Installer
    {
        public static IServiceCollection AddServer<TClientPeerHandlerFactory>(this IServiceCollection services)
            where TClientPeerHandlerFactory : class, IClientPeerHandlerFactory
        {
            services.AddSingleton<IServer, Server>()
                    .AddTransient<IClientPeer, ClientPeer>();

            services.AddSingleton<IClientPeerHandlerFactory, TClientPeerHandlerFactory>();

            services.TryAddSingleton<INetBufferPool, NetBufferPool>();

            return services;
        }
    }
}
