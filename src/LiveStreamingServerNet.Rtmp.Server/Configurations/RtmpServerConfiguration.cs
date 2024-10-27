namespace LiveStreamingServerNet.Rtmp.Server.Configurations
{
    /// <summary>
    /// Configuration options for the RTMP server.
    /// </summary>
    public class RtmpServerConfiguration
    {
        /// <summary>
        /// Gets or sets the chunk size for outgoing RTMP messages in bytes.
        /// Default: 60,000 bytes.
        /// </summary>
        public uint OutChunkSize { get; set; } = 60_000;

        /// <summary>
        /// Gets or sets the suggested bandwidth limit for peers in bytes per second.
        /// Used to inform clients about server's bandwidth capacity. 
        /// Default: 1MB/sec.
        /// </summary>
        public uint PeerBandwidth { get; set; } = 1024 * 1024;

        /// <summary>
        /// Gets or sets the size of the acknowledgement window in bytes.
        /// Controls how frequently clients should acknowledge received bytes. 
        /// Default: 250,000 bytes.
        /// </summary>
        public uint WindowAcknowledgementSize { get; set; } = 250_000;

        /// <summary>
        /// Gets or sets whether to cache GOP (Group of Pictures) for instant playback start.
        /// When enabled, new viewers receive the most recent GOP (keyframe and subsequent frames) immediately. 
        /// Default: true.
        /// </summary>
        public bool EnableGopCaching { get; set; } = true;

        /// <summary>
        /// Gets or sets the time window for batching media packets.
        /// Longer windows can improve efficiency but increase latency. 
        /// Default: TimeSpan.Zero (no batching).
        /// </summary>
        public TimeSpan MediaPacketBatchWindow { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Timeout duration for stream continuation after interruption.
        /// A value of Zero indicates no continuation allowed.
        /// Default: TimeSpan.Zero.
        /// </summary>
        public TimeSpan PublishStreamContinuationTimeout { get; set; } = TimeSpan.Zero;
    }
}
