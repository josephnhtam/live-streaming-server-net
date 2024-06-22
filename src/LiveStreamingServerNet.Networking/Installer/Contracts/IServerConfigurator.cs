using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Installer.Contracts
{
    public interface IServerConfigurator
    {
        IServiceCollection Services { get; }

        IServerConfigurator AddServerEventHandler<TServerEventHandler>()
            where TServerEventHandler : class, IServerEventHandler;

        IServerConfigurator ConfigureNetwork(Action<NetworkConfiguration>? configure);

        IServerConfigurator ConfigureSecurity(Action<SecurityConfiguration>? configure);

        IServerConfigurator ConfigureDataBufferPool(Action<DataBufferPoolConfiguration>? configure);

        IServerConfigurator ConfigureBufferPool(Action<BufferPoolConfiguration>? configure);
    }
}
