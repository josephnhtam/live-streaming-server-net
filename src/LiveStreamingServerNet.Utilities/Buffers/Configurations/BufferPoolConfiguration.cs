namespace LiveStreamingServerNet.Utilities.Buffers.Configurations
{
    public class BufferPoolConfiguration
    {
        public int MinBufferSize { get; set; } = 512;
        public int MaxBufferSize { get; set; } = 8 * 1024 * 1024;
        public int MaxBuffersPerBucket { get; set; } = Math.Max(1, Environment.ProcessorCount) * 50;
    }
}
