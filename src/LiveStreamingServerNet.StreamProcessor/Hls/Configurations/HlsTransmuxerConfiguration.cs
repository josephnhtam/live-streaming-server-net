using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Hls.Configurations
{
    /// <summary>
    /// Configuration settings for HLS transmuxing operations.
    /// </summary>
    public class HlsTransmuxerConfiguration
    {
        /// <summary>
        /// Gets or sets the name identifier for this transmuxer configuration.
        /// Default: "hls-transmuxer"
        /// </summary>
        public string Name { get; set; } = "hls-transmuxer";

        /// <summary>
        /// Gets or sets the number of segments to keep in the playlist.
        /// Default: 20 segments
        /// </summary>
        public int SegmentListSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets whether to delete segments that are no longer in the playlist.
        /// Default: true
        /// </summary>
        public bool DeleteOutdatedSegments { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum size of a single HLS segment in bytes.
        /// Default: 16MB (1024 * 1024 * 16)
        /// </summary>
        public int MaxSegmentSize { get; set; } = 1024 * 1024 * 16;

        /// <summary>
        /// Gets or sets the maximum buffer size for segment processing in bytes.
        /// Default: 1.5MB (1.5 * 1024 * 1024)
        /// </summary>
        public int MaxSegmentBufferSize { get; set; } = (int)(1.5 * 1024 * 1024);

        /// <summary>
        /// Gets or sets the minimum duration for a segment.
        /// Default: 0.75 seconds
        /// </summary>
        public TimeSpan MinSegmentLength { get; set; } = TimeSpan.FromSeconds(0.75);

        /// <summary>
        /// Gets or sets the segment duration for audio-only streams.
        /// Default: 2 seconds
        /// </summary>
        public TimeSpan AudioOnlySegmentLength { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Gets or sets the delay before cleaning up transmuxed files.
        /// Default: 30 seconds
        /// </summary>
        public TimeSpan? CleanupDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the resolver for determining HLS output paths.
        /// Default: creates a path in the format: "./output/{contextIdentifier}/output.m3u8".
        /// </summary>
        public IHlsOutputPathResolver OutputPathResolver { get; set; } = new DefaultHlsOutputPathResolver();

        /// <summary>
        /// Gets or sets the condition that determines when this transmuxer should be used.
        /// Default: always true.
        /// </summary>
        public IStreamProcessorCondition Condition { get; set; } = new DefaultStreamProcessorCondition();
    }
}
