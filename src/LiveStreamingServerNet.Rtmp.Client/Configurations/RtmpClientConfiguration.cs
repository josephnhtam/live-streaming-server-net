namespace LiveStreamingServerNet.Rtmp.Client.Configurations
{
    /// <summary>
    /// Configuration options for RTMP client behavior.
    /// </summary>
    public class RtmpClientConfiguration
    {
        /// <summary>
        /// Gets or sets the chunk size for incoming RTMP messages in bytes.
        /// Default: 1,000,000 bytes. (0 means no limit)
        /// </summary>
        public uint MaxInChunkSize { get; set; } = 1_000_000;

        /// <summary>
        /// Gets or sets the chunk size for outgoing RTMP messages in bytes.
        /// Default: 60,000 bytes
        /// </summary>
        public uint OutChunkSize { get; set; } = 60_000;

        /// <summary>
        /// Gets or sets the size of the acknowledgement window in bytes.
        /// Controls how frequently the server should acknowledge received bytes. 
        /// Default: 250,000 bytes
        /// </summary>
        public uint WindowAcknowledgementSize { get; set; } = 250_000;

        /// <summary>
        /// Maximum time to wait for RTMP handshake completion.
        /// Default: 15 seconds
        /// </summary>
        public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(15);
    }
}
