namespace LiveStreamingServerNet.StreamProcessor.Hls.Configurations
{
    /// <summary>
    /// Configuration settings for HLS uploader operations.
    /// </summary>
    public class HlsUploaderConfiguration
    {
        /// <summary>
        /// Gets or sets whether to delete media segments that are no longer referenced in the playlist.
        /// Default: true.
        /// </summary>
        public bool DeleteOutdatedSegments { get; set; } = true;

        /// <summary>
        /// Gets or sets the interval between polling for new segments.
        /// Default: 500 milliseconds.
        /// </summary>
        public TimeSpan PollingInterval => TimeSpan.FromMilliseconds(500);
    }
}
