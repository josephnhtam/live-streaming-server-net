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
            services.AddLogging()
                    .AddOptions();

            services.AddSingleton<IServer, Server>()
                    .AddSingleton<IServerHandle>(x => x.GetRequiredService<IServer>())
                    .AddTransient<IClient, Client>();

            services.AddSingleton<IServerEventDispatcher, ServerEventDispatcher>()
                    .AddSingleton<IClientHandlerFactory, TClientHandlerFactory>();

            services.TryAddSingleton<INetBufferPool, NetBufferPool>();

            return services;
        }
    }
}
