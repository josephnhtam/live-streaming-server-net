using LiveStreamingServerNet.StreamProcessor.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    /// <summary>
    /// Defines a configuration interface for setting up stream processing services.
    /// </summary>
    public interface IStreamProcessingConfigurator
    {
        /// <summary>
        /// Gets the service collection for dependency injection configuration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures a typed input path resolver for stream processing.
        /// </summary>
        /// <typeparam name="TInputPathResolver">Type of the input path resolver to use.</typeparam>
        /// <returns>The configurator instance for method chaining.</returns>
        IStreamProcessingConfigurator UseInputPathResolver<TInputPathResolver>()
            where TInputPathResolver : class, IInputPathResolver;

        /// <summary>
        /// Configures an input path resolver using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the input path resolver instance.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IStreamProcessingConfigurator UseInputPathResolver(Func<IServiceProvider, IInputPathResolver> implementationFactory);

        /// <summary>
        /// Registers a typed stream processor event handler.
        /// </summary>
        /// <typeparam name="TStreamProcessorEventHandler">Type of the stream processor event handler to register.</typeparam>
        /// <returns>The configurator instance for method chaining.</returns>
        IStreamProcessingConfigurator AddStreamProcessorEventHandler<TStreamProcessorEventHandler>()
            where TStreamProcessorEventHandler : class, IStreamProcessorEventHandler;

        /// <summary>
        /// Registers a stream processor event handler using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the event handler instance.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IStreamProcessingConfigurator AddStreamProcessorEventHandler(Func<IServiceProvider, IStreamProcessorEventHandler> implementationFactory);
    }
}
