using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Relay.Configurations
{
    /// <summary>
    /// Configuration settings for RTMP upstream relay functionality.
    /// </summary>
    public class RtmpUpstreamConfiguration
    {
        /// <summary>
        /// Gets or sets whether upstream relay is enabled.
        /// Default: true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the condition that determines when upstream relay should occur.
        /// If null, relay occurs unconditionally.
        /// </summary>
        public IRtmpUpstreamRelayCondition? Condition { get; set; } = null;

        /// <summary>
        /// Gets or sets the optimal number of media packets to keep in the outbound buffer.
        /// Default: 64 packets.
        /// </summary>
        public int TargetOutstandingMediaPacketsCount { get; set; } = 64;

        /// <summary>
        /// Gets or sets the optimal total size of media packets in the outbound buffer.
        /// Default: 1MB.
        /// </summary>
        public long TargetOutstandingMediaPacketsSize { get; set; } = 1024 * 1024;

        /// <summary>
        /// Gets or sets the maximum number of media packets allowed in the outbound buffer.
        /// Default: 512 packets.
        /// </summary>
        public int MaxOutstandingMediaPacketsCount { get; set; } = 512;

        /// <summary>
        /// Gets or sets the maximum total size of media packets allowed in the outbound buffer.
        /// Default: 8MB.
        /// </summary>
        public long MaxOutstandingMediaPacketsSize { get; set; } = 8 * 1024 * 1024;

        /// <summary>
        /// Gets or sets the maximum time an upstream connection can remain idle before being closed.
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
        /// Gets or sets the action to configure RTMP-specific settings for upstream connections.
        /// </summary>
        public Action<IRtmpClientConfigurator>? ConfigureRtmpUpstreamClient { get; set; } = null;

        /// <summary>
        /// Gets or sets the action to configure general client settings for upstream connections.
        /// </summary>
        public Action<IClientConfigurator>? ConfigureUpstreamClient { get; set; } = null;
    }
}
