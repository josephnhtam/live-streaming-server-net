using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    /// <summary>
    /// Builder interface for configuring and creating RTMP client instances.
    /// </summary>
    public interface IRtmpClientBuilder
    {
        /// <summary>
        /// Gets the service collection for dependency injection configuration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures logging for the RTMP client.
        /// </summary>
        /// <param name="configure">Action to configure logging options</param>
        /// <returns>The builder instance for method chaining</returns>
        IRtmpClientBuilder ConfigureLogging(Action<ILoggingBuilder> configure);

        /// <summary>
        /// Configures RTMP-specific settings.
        /// </summary>
        /// <param name="configure">Action to configure RTMP options</param>
        /// <returns>The builder instance for method chaining</returns>
        IRtmpClientBuilder ConfigureRtmpClient(Action<IRtmpClientConfigurator> configure);

        /// <summary>
        /// Configures general client settingsr.
        /// </summary>
        /// <param name="configure">Action to configure client options</param>
        /// <returns>The builder instance for method chaining</returns>
        IRtmpClientBuilder ConfigureClient(Action<IClientConfigurator> configure);

        /// <summary>
        /// Creates and returns a configured RTMP client instance.
        /// </summary>
        /// <returns>A new RTMP client configured with the specified options</returns>
        IRtmpClient Build();
    }
}
