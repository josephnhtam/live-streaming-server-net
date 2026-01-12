using LiveStreamingServerNet.WebRTC.Stun;

namespace LiveStreamingServerNet.WebRTC.Ice.Configurations
{
    public class IceAgentConfiguration
    {
        /// <summary>
        /// Interval for connectivity checks.
        /// Default: 50 milliseconds.
        /// </summary>
        public TimeSpan ConnectivityCheckInterval { get; set; } = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Interval for keeping alive.
        /// Default: 5 seconds.
        /// </summary>
        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Maximum size of the checklist.
        /// Default: 100.
        /// </summary>
        public int MaxCheckListSize { get; set; } = 100;

        /// <summary>
        /// Maximum number of concurrent connectivity checks.
        /// Default: 0. (0 means no limit).
        /// </summary>
        public int MaxConcurrentConnectivityChecks { get; set; } = 0;

        /// <summary>
        /// Connectivity check retransmission options.
        /// </summary>
        public StunRetransmissionOptions ConnectivityCheckRetransmissionOptions { get; set; } =
            new StunRetransmissionOptions
            {
                RetransmissionTimeout = TimeSpan.FromMilliseconds(50),
                MaxRetransmissionTimeout = TimeSpan.FromMilliseconds(1600),
                MaxRetransmissions = 7,
                TransactionTimeoutFactor = 16
            };

        /// <summary>
        /// Keep-alive retransmission options.
        /// </summary>
        public StunRetransmissionOptions KeepAliveRetransmissionOptions { get; set; } =
            new StunRetransmissionOptions
            {
                RetransmissionTimeout = TimeSpan.FromMilliseconds(50),
                MaxRetransmissionTimeout = TimeSpan.FromMilliseconds(1600),
                MaxRetransmissions = 14,
                TransactionTimeoutFactor = 16
            };
    }
}
