using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Networking.Server.Installer.Contracts;
using LiveStreamingServerNet.Networking.Server.Internal;
using LiveStreamingServerNet.Networking.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Networking.Server.Installer
{
    public static class ServerInstaller
    {
        public static IServiceCollection AddServer<TClientSessionHandlerFactory>(
            this IServiceCollection services, Action<IServerConfigurator>? configure = null)
            where TClientSessionHandlerFactory : class, ISessionHandlerFactory
        {
            services.AddLogging()
                    .AddOptions();

            services.AddSingleton<IServer, Internal.Server>()
                    .AddSingleton<IServerHandle>(x => x.GetRequiredService<IServer>())
                    .AddSingleton<ITcpListenerFactory, TcpListenerFactory>();

            services.AddSingleton<IClientSessionManager, ClientSessionManager>()
                    .AddSingleton<ISessionFactory, ClientSessionFactory>()
                    .AddSingleton<IBufferSenderFactory, BufferSenderFactory>()
                    .AddSingleton<INetworkStreamFactory, NetworkStreamFactory>()
                    .AddSingleton<ISessionHandlerFactory, TClientSessionHandlerFactory>()
                    .AddSingleton<ISslStreamFactory, SslStreamFactory>();

            services.AddSingleton<IServerEventDispatcher, ServerEventDispatcher>();

            configure?.Invoke(new ServerConfigurator(services));

            services.TryAddSingleton<IDataBufferPool, DataBufferPool>();
            services.TryAddSingleton<IBufferPool, BufferPool>();

            return services;
        }
    }
}
