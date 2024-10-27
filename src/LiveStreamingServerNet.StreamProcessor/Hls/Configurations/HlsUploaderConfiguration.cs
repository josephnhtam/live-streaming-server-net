namespace LiveStreamingServerNet.StreamProcessor.Hls.Configurations
{
    /// <summary>
    /// Configuration settings for HLS uploader operations.
    /// </summary>
    public class HlsUploaderConfiguration
    {
        /// <summary>
        /// Gets or sets whether to delete transport stream segments that are no longer referenced in the playlist.
        /// Default: true.
        /// </summary>
        public bool DeleteOutdatedTsSegments { get; set; } = true;

        /// <summary>
        /// Gets or sets the interval between polling for new segments.
        /// Default: 500 milliseconds.
        /// </summary>
        public TimeSpan PollingInterval => TimeSpan.FromMilliseconds(500);
    }
}
