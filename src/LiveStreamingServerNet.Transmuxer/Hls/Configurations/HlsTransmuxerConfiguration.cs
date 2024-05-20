using LiveStreamingServerNet.Transmuxer.Hls.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Hls.Configurations
{
    public class HlsTransmuxerConfiguration
    {
        public string Name { get; set; } = "hls-transmuxer";
        public IHlsOutputPathResolver OutputPathResolver { get; set; } = new DefaultHlsOutputPathResolver();
    }
}
