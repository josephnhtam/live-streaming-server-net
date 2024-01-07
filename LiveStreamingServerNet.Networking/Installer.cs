using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Newtorking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Networking
{
    public static class Installer
    {
        public static IServiceCollection AddServer<TClientHandlerFactory>(this IServiceCollection services)
            where TClientHandlerFactory : class, IClientHandlerFactory
        {
            services.AddSingleton<IServer, Server>()
                    .AddTransient<IClient, Client>();

            services.AddSingleton<IClientHandlerFactory, TClientHandlerFactory>();

            services.TryAddSingleton<INetBufferPool, NetBufferPool>();

            return services;
        }
    }
}
