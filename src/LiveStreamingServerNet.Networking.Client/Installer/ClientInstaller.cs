using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Networking.Client.Internal;
using LiveStreamingServerNet.Networking.Client.Internal.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Networking.Client.Installer
{
    public static class ServerInstaller
    {
        public static IServiceCollection AddClient<TSessionHandlerFactory>(
            this IServiceCollection services, Action<IClientConfigurator>? configure = null)
            where TSessionHandlerFactory : class, ISessionHandlerFactory
        {
            services.AddLogging()
                    .AddOptions();

            services.AddSingleton<IClient, Internal.Client>()
                    .AddSingleton<ITcpClientFactory, TcpClientFactory>();

            services.AddSingleton<ISessionFactory, SessionFactory>()
                    .AddSingleton<IBufferSenderFactory, BufferSenderFactory>()
                    .AddSingleton<INetworkStreamFactory, NetworkStreamFactory>()
                    .AddSingleton<ISessionHandlerFactory, TSessionHandlerFactory>()
                    .AddSingleton<ISslStreamFactory, SslStreamFactory>();

            services.AddSingleton<IClientEventDispatcher, ClientEventDispatcher>();

            configure?.Invoke(new ClientConfigurator(services));

            services.TryAddSingleton<IDataBufferPool, DataBufferPool>();
            services.TryAddSingleton<IBufferPool, BufferPool>();

            return services;
        }
    }
}
