namespace LiveStreamingServerNet.Newtorking.Configurations
{
    public class NetBufferPoolConfiguration
    {
        public int NetBufferCapacity { get; set; } = 1024;
        public int MaxPoolSize { get; set; } = -1;
    }
}
