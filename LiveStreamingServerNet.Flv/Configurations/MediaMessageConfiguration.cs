namespace LiveStreamingServerNet.Flv.Configurations
{
    public class MediaMessageConfiguration
    {
        public int TargetOutstandingMediaMessageCount { get; set; } = 8;
        public long TargetOutstandingMediaMessageSize { get; set; } = 128 * 1024;
        public int MaxOutstandingMediaMessageCount { get; set; } = 24;
        public long MaxOutstandingMediaMessageSize { get; set; } = 1024 * 1024;
    }
}
