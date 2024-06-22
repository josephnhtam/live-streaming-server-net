using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
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
                    .AddSingleton<ITcpListenerFactory, TcpListenerFactory>();

            services.AddSingleton<IClientManager, ClientManager>()
                    .AddSingleton<IClientFactory, ClientFactory>()
                    .AddSingleton<IClientBufferSenderFactory, ClientBufferSenderFactory>()
                    .AddSingleton<INetworkStreamFactory, NetworkStreamFactory>()
                    .AddSingleton<IClientHandlerFactory, TClientHandlerFactory>()
                    .AddSingleton<ISslStreamFactory, SslStreamFactory>();

            services.AddSingleton<IServerEventDispatcher, ServerEventDispatcher>();

            services.TryAddSingleton<IDataBufferPool, DataBufferPool>();
            services.TryAddSingleton<IBufferPool, BufferPool>();

            configure?.Invoke(new ServerConfigurator(services));

            return services;
        }
    }
}
