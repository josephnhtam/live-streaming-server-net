using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Relay.Configurations
{
    /// <summary>
    /// Configuration settings for RTMP downstream relay functionality.
    /// </summary>
    public class RtmpDownstreamConfiguration
    {
        /// <summary>
        /// Gets or sets whether downstream relay is enabled.
        /// Default: true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the condition that determines when downstream relay should occur.
        /// If null, relay occurs unconditionally.
        /// </summary>
        public IRtmpDownstreamRelayCondition? Condition { get; set; } = null;

        /// <summary>
        /// Gets or sets the maximum time a downstream connection can remain idle before being closed.
        /// Default: 30 seconds.
        /// </summary>
        public TimeSpan MaximumIdleTime { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets how frequently to check for idle connections.
        /// Default: 3 seconds.
        /// </summary>
        public TimeSpan IdleCheckingInterval { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Gets or sets the retry settings for reconnection attempts when connection is lost.
        /// Default: unlimited retries starting at 100ms, capped at 1000ms, with exponential backoff factor of 2.
        /// </summary>
        public RetrySettings ReconnectSettings { get; set; } =
            new RetrySettings(int.MaxValue, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), 2);

        /// <summary>
        /// Gets or sets the action to configure RTMP-specific settings for downstream connections.
        /// </summary>
        public Action<IRtmpClientConfigurator>? ConfigureRtmpDownstreamClient { get; set; } = null;

        /// <summary>
        /// Gets or sets the action to configure general client settings for downstream connections.
        /// </summary>
        public Action<IClientConfigurator>? ConfigureDownstreamClient { get; set; } = null;
    }
}
