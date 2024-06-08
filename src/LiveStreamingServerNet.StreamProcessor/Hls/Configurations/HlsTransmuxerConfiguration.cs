using LiveStreamingServerNet.StreamProcessor.Contracts;
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
        public TimeSpan MinSegmentLength { get; set; } = TimeSpan.FromSeconds(0.75);
        public TimeSpan AudioOnlySegmentLength { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan? CleanupDelay { get; set; } = TimeSpan.FromSeconds(30);
        public IHlsOutputPathResolver OutputPathResolver { get; set; } = new DefaultHlsOutputPathResolver();
        public IStreamProcessorCondition Condition { get; set; } = new DefaultStreamProcessorCondition();
    }
}
