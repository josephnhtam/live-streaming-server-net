using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Server.Configurations;
using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Server.Installer.Contracts
{
    public interface IServerConfigurator
    {
        IServiceCollection Services { get; }

        IServerConfigurator AddServerEventHandler<TServerEventHandler>()
            where TServerEventHandler : class, IServerEventHandler;

        IServerConfigurator AddServerEventHandler<TServerEventHandler>(Func<IServiceProvider, IServerEventHandler> factory);

        IServerConfigurator ConfigureNetwork(Action<NetworkConfiguration>? configure);

        IServerConfigurator ConfigureSecurity(Action<SecurityConfiguration>? configure);

        IServerConfigurator ConfigureDataBufferPool(Action<DataBufferPoolConfiguration>? configure);

        IServerConfigurator ConfigureBufferPool(Action<BufferPoolConfiguration>? configure);
    }
}
