using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Hls.Configurations
{
    public class HlsTransmuxerConfiguration
    {
        public string Name { get; set; } = "hls-transmuxer";
        public int SegmentListSize { get; set; } = 20;
        public bool DeleteOutdatedSegments { get; set; } = true;
        public int MaxSegmentSize { get; set; } = 1024 * 1024 * 16;
        public int MaxSegmentBufferSize { get; set; } = (int)(1.5 * 1024 * 1024);
        public int AudioOnlySegmentDuration { get; set; } = 2000;
        public IHlsOutputPathResolver OutputPathResolver { get; set; } = new DefaultHlsOutputPathResolver();
    }
}
