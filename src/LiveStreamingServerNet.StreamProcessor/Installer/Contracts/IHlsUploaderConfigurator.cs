using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    /// <summary>
    /// Defines a configuration interface for setting up HLS uploaders.
    /// </summary>
    public interface IHlsUploaderConfigurator
    {
        /// <summary>
        /// Gets the service collection for dependency injection configuration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Registers a typed HLS storage event handler.
        /// </summary>
        /// <typeparam name="THlsStorageEventHandler">Type of the HLS storage event handler to register.</typeparam>
        /// <returns>The configurator instance for method chaining.</returns>
        IHlsUploaderConfigurator AddHlsStorageEventHandler<THlsStorageEventHandler>()
            where THlsStorageEventHandler : class, IHlsStorageEventHandler;

        /// <summary>
        /// Registers an HLS storage event handler using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the event handler instance.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IHlsUploaderConfigurator AddHlsStorageEventHandler(Func<IServiceProvider, IHlsStorageEventHandler> implementationFactory);

        /// <summary>
        /// Registers a typed HLS uploader condition.
        /// </summary>
        /// <typeparam name="THlsUploaderCondition">Type of the HLS uploader condition to register.</typeparam>
        /// <returns>The configurator instance for method chaining.</returns>
        IHlsUploaderConfigurator AddHlsUploaderCondition<THlsUploaderCondition>()
            where THlsUploaderCondition : class, IHlsUploaderCondition;

        /// <summary>
        /// Registers an HLS uploader condition using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the uploader condition instance.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IHlsUploaderConfigurator AddHlsUploaderCondition(Func<IServiceProvider, IHlsUploaderCondition> implementationFactory);
    }
}
