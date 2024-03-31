using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Networking.Installer
{
    public static class ServerInstaller
    {
        public static IServiceCollection AddServer<TClientHandlerFactory>(this IServiceCollection services, Action<IServerConfigurator>? configure = null)
            where TClientHandlerFactory : class, IClientHandlerFactory
        {
            services.AddLogging()
                    .AddOptions();

            services.AddSingleton<IServer, Server>()
                    .AddSingleton<IServerHandle>(x => x.GetRequiredService<IServer>())
                    .AddSingleton<IClientFactory, ClientFactory>();

            services.AddSingleton<IServerEventDispatcher, ServerEventDispatcher>()
                    .AddSingleton<IClientHandlerFactory, TClientHandlerFactory>();

            services.TryAddSingleton<INetBufferPool, NetBufferPool>();

            configure?.Invoke(new ServerConfigurator(services));

            return services;
        }
    }
}
