namespace LiveStreamingServerNet.Rtmp.Server.Configurations
{
    /// <summary>
    /// Configuration settings for media streaming buffer management.
    /// </summary>
    public class MediaStreamingConfiguration
    {
        /// <summary>
        /// Target number of media packets to maintain in the buffer.
        /// Default: 64 packets.
        /// </summary>
        public int TargetOutstandingMediaPacketsCount { get; set; } = 64;

        /// <summary>
        /// Target total size of buffered media packets in bytes.
        /// Default: 1MB (1,048,576 bytes).
        /// </summary>
        public long TargetOutstandingMediaPacketsSize { get; set; } = 1024 * 1024;

        /// <summary>
        /// Maximum number of media packets allowed in the buffer.
        /// When exceeded, newer packets will be dropped.
        /// Default: 512 packets.
        /// </summary>
        public int MaxOutstandingMediaPacketsCount { get; set; } = 512;

        /// <summary>
        /// Maximum total size of buffered media packets in bytes.
        /// When exceeded, newer packets will be dropped.
        /// Default: 8MB (8,388,608 bytes).
        /// </summary>
        public long MaxOutstandingMediaPacketsSize { get; set; } = 8 * 1024 * 1024;

        /// <summary>
        /// Maximum size of the GOP (Group of Pictures) cache in bytes.
        /// Cached GOPs ensure immediate availability of the first frame for new viewers.
        /// Default: 16MB (16,777,216 bytes).
        /// </summary>
        public long MaxGroupOfPicturesCacheSize { get; set; } = 16 * 1024 * 1024;
    }
}
