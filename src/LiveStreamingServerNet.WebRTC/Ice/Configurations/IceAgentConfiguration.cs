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
        /// Default: 256.
        /// </summary>
        public int MaxConcurrentConnectivityChecks { get; set; } = 256;
    }
}
