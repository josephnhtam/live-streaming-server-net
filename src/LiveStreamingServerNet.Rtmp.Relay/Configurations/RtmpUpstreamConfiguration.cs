using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Relay.Configurations
{
    public class RtmpUpstreamConfiguration
    {
        public bool Enabled { get; set; } = true;
        public IRtmpUpstreamRelayCondition? Condition { get; set; } = null;

        public int TargetOutstandingMediaPacketsCount { get; set; } = 64;
        public long TargetOutstandingMediaPacketsSize { get; set; } = 1024 * 1024;
        public int MaxOutstandingMediaPacketsCount { get; set; } = 512;
        public long MaxOutstandingMediaPacketsSize { get; set; } = 8 * 1024 * 1024;

        public TimeSpan MaximumIdleTime { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan IdleCheckingInterval { get; set; } = TimeSpan.FromSeconds(3);
        public RetrySettings ReconnectSettings { get; set; } = new RetrySettings(int.MaxValue, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), 2);

        public Action<IRtmpClientConfigurator>? ConfigureRtmpUpstreamClient { get; set; } = null;
        public Action<IClientConfigurator>? ConfigureUpstreamClient { get; set; } = null;
    }
}
