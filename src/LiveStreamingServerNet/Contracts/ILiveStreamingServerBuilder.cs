using LiveStreamingServerNet.Networking.Server.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Contracts
{
    /// <summary>
    /// Interface for building and configuring a live streaming server instance
    /// </summary>
    public interface ILiveStreamingServerBuilder
    {
        /// <summary>
        /// Gets the service collection for dependency injection configuration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures the logging for the streaming server.
        /// </summary>
        /// <param name="configure">Action to configure the logging builder.</param>
        /// <returns>The server builder instance for method chaining.</returns>
        ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configure);

        /// <summary>
        /// Configures the RTMP server settings and behavior.
        /// </summary>
        /// <param name="configure">Action to configure the RTMP server.</param>
        /// <returns>The server builder instance for method chaining.</returns> 
        ILiveStreamingServerBuilder ConfigureRtmpServer(Action<IRtmpServerConfigurator> configure);

        /// <summary>
        /// Configures general server settings.
        /// </summary>
        /// <param name="configure">Action to configure the server.</param>
        /// <returns>The server builder instance for method chaining.</returns>
        ILiveStreamingServerBuilder ConfigureServer(Action<IServerConfigurator> configure);

        /// <summary>
        /// Builds and returns a configured instance of the live streaming server.
        /// </summary>
        /// <returns>The configured live streaming server instance.</returns>
        ILiveStreamingServer Build();
    }
}