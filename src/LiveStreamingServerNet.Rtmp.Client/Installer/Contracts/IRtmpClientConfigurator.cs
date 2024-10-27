using LiveStreamingServerNet.Rtmp.Client.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer.Contracts
{
    /// <summary>
    /// Interface for configuring RTMP client settings and services.
    /// </summary>
    public interface IRtmpClientConfigurator
    {
        /// <summary>
        /// Gets the service collection for registering dependencies.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures RTMP client settings.
        /// </summary>
        /// <param name="configure">Action to modify RTMP client configuration.</param>
        /// <returns>The configurator instance for method chaining</returns>
        IRtmpClientConfigurator Configure(Action<RtmpClientConfiguration>? configure);
    }
}