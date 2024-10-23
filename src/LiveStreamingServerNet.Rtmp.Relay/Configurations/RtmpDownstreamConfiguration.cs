using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Relay.Configurations
{
    public class RtmpDownstreamConfiguration
    {
        public bool Enabled { get; set; } = true;
        public IRtmpDownstreamRelayCondition? Condition { get; set; } = null;

        public TimeSpan MaximumIdleTime { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan IdleCheckingInterval { get; set; } = TimeSpan.FromSeconds(3);
        public RetrySettings ReconnectSettings { get; set; } = new RetrySettings(int.MaxValue, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), 2);

        public Action<IRtmpClientConfigurator>? ConfigureRtmpDownstreamClient { get; set; } = null;
        public Action<IClientConfigurator>? ConfigureDownstreamClient { get; set; } = null;
    }
}
