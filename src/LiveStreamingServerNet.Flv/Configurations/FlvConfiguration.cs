namespace LiveStreamingServerNet.Flv.Configurations
{
    /// <summary>
    /// Configuration settings for FLV streaming
    /// </summary>
    public class FlvConfiguration
    {
        /// <summary>
        /// Maximum time to wait for a stream to become ready (receiving the first non-header packet).
        /// Default: 15 seconds.
        /// </summary>
        public TimeSpan ReadinessTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Timeout duration for stream continuation after interruption.
        /// A value of Zero indicates no continuation allowed.
        /// Default: TimeSpan.Zero.
        /// </summary>
        public TimeSpan StreamContinuationTimeout { get; set; } = TimeSpan.Zero;
    }
}
