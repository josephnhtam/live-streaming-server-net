using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Newtorking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Networking
{
    public static class Installer
    {
        public static IServiceCollection AddServer(this IServiceCollection services)
        {
            services.AddSingleton<IServer, Server>()
                    .AddTransient<IClientPeer, ClientPeer>();

            services.TryAddSingleton<INetBufferPool, NetBufferPool>();

            return services;
        }
    }
}
