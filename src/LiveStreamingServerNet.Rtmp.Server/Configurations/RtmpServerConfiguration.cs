namespace LiveStreamingServerNet.Rtmp.Server.Configurations
{
    public class RtmpServerConfiguration
    {
        public uint OutChunkSize { get; set; } = 60_000;
        public uint PeerBandwidth { get; set; } = 500_000;
        public uint WindowAcknowledgementSize { get; set; } = 250_000;
        public bool EnableGopCaching { get; set; } = true;
        public TimeSpan MediaPacketBatchWindow { get; set; } = TimeSpan.Zero;
    }
}
