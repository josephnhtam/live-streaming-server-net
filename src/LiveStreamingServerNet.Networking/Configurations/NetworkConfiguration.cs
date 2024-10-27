namespace LiveStreamingServerNet.Networking.Configurations
{
    /// <summary>
    /// Configuration settings for network socket behavior.
    /// </summary>
    public class NetworkConfiguration
    {
        /// <summary>
        /// When true, uses inline completions for socket I/O operations on non-Windows systems.
        /// This can improve CPU usage on Linux/Unix systems, especaially when the system is not under heavy load.
        /// Default: true.
        /// </summary>
        public bool PreferInlineCompletionsOnNonWindows { get; set; } = true;

        /// <summary>
        /// Size of the socket receive buffer in bytes.
        /// Default: 1MB (1,048,576 bytes).
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1024 * 1024;

        /// <summary>
        /// Size of the socket send buffer in bytes.
        /// Default: 1MB (1,048,576 bytes).
        /// </summary>
        public int SendBufferSize { get; set; } = 1024 * 1024;

        /// <summary>
        /// Disables Nagle's algorithm when set to true.
        /// False keeps Nagle's algorithm enabled for better bandwidth utilization.
        /// Default: false.
        /// </summary>
        public bool NoDelay { get; set; } = false;
    }
}
