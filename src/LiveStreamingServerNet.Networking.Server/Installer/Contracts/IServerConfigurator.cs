using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Server.Configurations;
using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Server.Installer.Contracts
{
    /// <summary>
    /// Configuration interface for TCP server setup.
    /// </summary>
    public interface IServerConfigurator
    {
        /// <summary>
        /// Gets the service collection for dependency registration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds a server event handler using dependency injection.
        /// </summary>
        /// <typeparam name="TServerEventHandler">Type of event handler to add.</typeparam>
        IServerConfigurator AddServerEventHandler<TServerEventHandler>()
            where TServerEventHandler : class, IServerEventHandler;

        /// <summary>
        /// Adds a server event handler using a factory function.
        /// </summary>
        /// <param name="factory">Factory function to create handler instance.</param>
        IServerConfigurator AddServerEventHandler<TServerEventHandler>(Func<IServiceProvider, IServerEventHandler> factory);

        /// <summary>
        /// Configures network-related settings.
        /// </summary>
        /// <param name="configure">Action to configure network options.</param>
        IServerConfigurator ConfigureNetwork(Action<NetworkConfiguration>? configure);

        /// <summary>
        /// Configures SSL/TLS security settings.
        /// </summary>
        /// <param name="configure">Action to configure security options.</param>
        IServerConfigurator ConfigureSecurity(Action<SecurityConfiguration>? configure);

        /// <summary>
        /// Configures data buffer pool settings.
        /// </summary>
        /// <param name="configure">Action to configure data buffer pool options.</param>
        IServerConfigurator ConfigureDataBufferPool(Action<DataBufferPoolConfiguration>? configure);

        /// <summary>
        /// Configures buffer pool settings.
        /// </summary>
        /// <param name="configure">Action to configure buffer pool options.</param>
        IServerConfigurator ConfigureBufferPool(Action<BufferPoolConfiguration>? configure);
    }
}
