using LiveStreamingServerNet.Networking.Client.Configurations;
using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Networking.Client.Installer.Contracts
{
    /// <summary>
    /// Configuration interface for TCP client setup.
    /// </summary>
    public interface IClientConfigurator
    {
        /// <summary>
        /// Gets the service collection for dependency registration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures network-related settings.
        /// </summary>
        /// <param name="configure">Action to configure network options.</param>
        IClientConfigurator ConfigureNetwork(Action<NetworkConfiguration>? configure);

        /// <summary>
        /// Configures SSL/TLS security settings.
        /// </summary>
        /// <param name="configure">Action to configure security options.</param>
        IClientConfigurator ConfigureSecurity(Action<SecurityConfiguration>? configure);

        /// <summary>
        /// Configures data buffer pool settings.
        /// </summary>
        /// <param name="configure">Action to configure data buffer pool options.</param>
        IClientConfigurator ConfigureDataBufferPool(Action<DataBufferPoolConfiguration>? configure);

        /// <summary>
        /// Configures buffer pool settings.
        /// </summary>
        /// <param name="configure">Action to configure buffer pool options.</param>
        IClientConfigurator ConfigureBufferPool(Action<BufferPoolConfiguration>? configure);

        /// <summary>
        /// Adds a server certificate validator using dependency injection.
        /// </summary>
        /// <typeparam name="TServerCertificateValidator">Type of certificate validator to add.</typeparam>
        IClientConfigurator AddServerCertificateValidator<TServerCertificateValidator>()
            where TServerCertificateValidator : class, IServerCertificateValidator;

        /// <summary>
        /// Adds a server certificate validator using a factory function.
        /// </summary>
        /// <param name="factory">Factory function to create validator instance.</param>
        IClientConfigurator AddServerCertificateValidator(Func<IServiceProvider, IServerCertificateValidator> factory);

        /// <summary>
        /// Adds a client event handler using dependency injection.
        /// </summary>
        /// <typeparam name="TServerEventHandler">Type of event handler to add.</typeparam>
        IClientConfigurator AddClientEventHandler<TServerEventHandler>()
            where TServerEventHandler : class, IClientEventHandler;

        /// <summary>
        /// Adds a client event handler using a factory function.
        /// </summary>
        /// <param name="factory">Factory function to create handler instance.</param>
        IClientConfigurator AddClientEventHandler<TServerEventHandler>(Func<IServiceProvider, IClientEventHandler> factory);
    }
}
