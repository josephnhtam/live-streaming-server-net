using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Newtorking.Configurations;
using LiveStreamingServerNet.Newtorking.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Installer
{
    public class ServerConfigurator : IServerConfigurator
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

        public IServerConfigurator ConfigureNetBufferPool(Action<NetBufferPoolConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }
    }
}
