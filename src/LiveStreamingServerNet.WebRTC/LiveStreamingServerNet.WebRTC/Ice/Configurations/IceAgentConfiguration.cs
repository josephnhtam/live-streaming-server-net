namespace LiveStreamingServerNet.WebRTC.Ice.Configurations
{
    public class IceAgentConfiguration
    {
        /// <summary>
        /// Interval for connectivity checks.
        /// </summary>
        public TimeSpan ConnectivityCheckInterval { get; set; } = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Interval for keeping alive.
        /// </summary>
        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);
    }
}
