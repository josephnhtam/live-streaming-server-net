namespace LiveStreamingServerNet.Utilities.Buffers.Configurations
{
    public class DataBufferPoolConfiguration
    {
        public int BufferInitialCapacity { get; set; } = 8192;
        public int MaxPoolSize { get; set; } = -1;
    }
}
