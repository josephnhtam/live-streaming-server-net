namespace LiveStreamingServerNet.Rtmp.Client.Configurations
{
    public class RtmpClientConfiguration
    {
        public uint OutChunkSize { get; set; } = 60_000;
        public uint WindowAcknowledgementSize { get; set; } = 250_000;

        public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(15);
    }
}
