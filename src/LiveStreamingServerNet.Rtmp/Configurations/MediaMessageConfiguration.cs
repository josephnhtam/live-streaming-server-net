namespace LiveStreamingServerNet.Rtmp.Configurations
{
    public class MediaMessageConfiguration
    {
        public int TargetOutstandingMediaMessageCount { get; set; } = 64;
        public long TargetOutstandingMediaMessageSize { get; set; } = 1024 * 1024;
        public int MaxOutstandingMediaMessageCount { get; set; } = 512;
        public long MaxOutstandingMediaMessageSize { get; set; } = 8 * 1024 * 1024;
        public long MaxGroupOfPicturesCacheSize { get; set; } = 16 * 1024 * 1024;
    }
}
