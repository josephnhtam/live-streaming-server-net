namespace LiveStreamingServerNet.Rtmp.Configurations
{
    public class RtmpServerConfiguration
    {
        public uint OutChunkSize { get; set; } = 60_000;
        public uint ClientBandwidth { get; set; } = 500_000;
        public uint OutAcknowledgementWindowSize { get; set; } = 250_000;
        public bool EnableGopCaching { get; set; } = true;
        public TimeSpan MediaPackageBatchWindow { get; set; } = TimeSpan.Zero;
    }
}
