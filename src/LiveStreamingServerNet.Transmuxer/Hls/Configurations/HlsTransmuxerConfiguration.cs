using LiveStreamingServerNet.Transmuxer.Hls.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Hls.Configurations
{
    public class HlsTransmuxerConfiguration
    {
        public string Name { get; set; } = "hls-transmuxer";
        public int SegmentListSize { get; set; } = 20;
        public bool DeleteOutdatedSegments { get; set; } = true;
        public IHlsOutputPathResolver OutputPathResolver { get; set; } = new DefaultHlsOutputPathResolver();
    }
}
