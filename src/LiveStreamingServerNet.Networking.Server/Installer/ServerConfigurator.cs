using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Server.Configurations;
using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Networking.Server.Installer.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Server.Installer
{
    internal class ServerConfigurator : IServerConfigurator
    {
        public IServiceCollection Services { get; }

        public ServerConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IServerConfigurator AddServerEventHandler<TServerEventHandler>()
            where TServerEventHandler : class, IServerEventHandler
        {
            Services.AddSingleton<IServerEventHandler, TServerEventHandler>();
            return this;
        }

        public IServerConfigurator AddServerEventHandler<TServerEventHandler>(Func<IServiceProvider, IServerEventHandler> factory)
        {
            Services.AddSingleton(factory);
            return this;
        }

        public IServerConfigurator ConfigureNetwork(Action<NetworkConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public IServerConfigurator ConfigureSecurity(Action<SecurityConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public IServerConfigurator ConfigureDataBufferPool(Action<DataBufferPoolConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public IServerConfigurator ConfigureBufferPool(Action<BufferPoolConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }
    }
}
