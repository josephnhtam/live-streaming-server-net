using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Utilities.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Installer
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

        public IServerConfigurator ConfigureNetBufferPool(Action<NetBufferPoolConfiguration>? configure)
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
