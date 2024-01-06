namespace LiveStreamingServerNet.Rtmp.Configurations
{
    public class RtmpServerConfiguration
    {
        public uint OutChunkSize { get; set; } = 60_000;
        public uint PeerBandwidth { get; set; } = 500_000;
        public uint OutAcknowledgementWindowSize { get; set; } = 250_000;
    }
}
