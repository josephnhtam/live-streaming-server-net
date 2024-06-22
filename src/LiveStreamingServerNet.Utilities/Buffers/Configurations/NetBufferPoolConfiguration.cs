namespace LiveStreamingServerNet.Utilities.Buffers.Configurations
{
    public class NetBufferPoolConfiguration
    {
        public int NetBufferCapacity { get; set; } = 4096;
        public int MaxPoolSize { get; set; } = -1;
    }
}
