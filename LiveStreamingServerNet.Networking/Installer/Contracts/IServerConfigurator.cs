using LiveStreamingServerNet.Newtorking.Configurations;
using LiveStreamingServerNet.Newtorking.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Installer.Contracts
{
    public interface IServerConfigurator
    {
        IServiceCollection Services { get; }

        IServerConfigurator AddServerEventHandler<TServerEventHandler>()
            where TServerEventHandler : class, IServerEventHandler;

        IServerConfigurator ConfigureNetBufferPool(Action<NetBufferPoolConfiguration>? configure);
    }
}
