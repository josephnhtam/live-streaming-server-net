using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Contracts;

namespace LiveStreamingServerNet.Flv.Installer.Contracts
{
    /// <summary>
    /// Interface for configuring FLV streaming settings.
    /// </summary>
    public interface IFlvConfigurator
    {
        /// <summary>
        /// Configures general FLV streaming options like timeouts.
        /// </summary>
        /// <param name="configure">Action to modify the FLV configuration.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IFlvConfigurator Configure(Action<FlvConfiguration>? configure);

        /// <summary>
        /// Configures media streaming settings like buffer sizes and packet limits.
        /// </summary>
        /// <param name="configure">Action to modify the media streaming configuration.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IFlvConfigurator ConfigureMediaStreaming(Action<MediaStreamingConfiguration>? configure);

        /// <summary>
        /// Adds a stream event handler implementation.
        /// </summary>
        /// <typeparam name="TStreamEventHandler">Type of the stream event handler</typeparam>
        IFlvConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IFlvServerStreamEventHandler;

        /// <summary>
        /// Adds a stream event handler using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the handler</param>
        IFlvConfigurator AddStreamEventHandler(Func<IServiceProvider, IFlvServerStreamEventHandler> implementationFactory);
    }
}
