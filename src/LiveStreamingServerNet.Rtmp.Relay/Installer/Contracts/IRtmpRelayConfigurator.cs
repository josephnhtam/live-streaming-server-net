using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Relay.Installer.Contracts
{
    /// <summary>
    /// Provides configuration options for RTMP relay functionality.
    /// </summary>
    public interface IRtmpRelayConfigurator
    {
        /// <summary>
        /// Gets the service collection for dependency injection configuration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures settings for upstream relays (when streams are received and republished to other servers).
        /// </summary>
        /// <param name="configure">Action to modify upstream settings, or null to use defaults</param>
        /// <returns>The configurator instance for method chaining</returns>
        IRtmpRelayConfigurator ConfigureUpstream(Action<RtmpUpstreamConfiguration>? configure);

        /// <summary>
        /// Configures settings for downstream relays (when streams are pulled from other servers).
        /// </summary>
        /// <param name="configure">Action to modify downstream settings, or null to use defaults</param>
        /// <returns>The configurator instance for method chaining</returns>
        IRtmpRelayConfigurator ConfigureDownstream(Action<RtmpDownstreamConfiguration>? configure);
    }
}
