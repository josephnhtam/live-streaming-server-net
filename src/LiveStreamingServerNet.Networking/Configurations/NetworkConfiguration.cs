namespace LiveStreamingServerNet.Networking.Configurations
{
    public class NetworkConfiguration
    {
        public int ReceiveBufferSize { get; set; } = 65536;
        public int SendBufferSize { get; set; } = 65536;
        public bool EnableNagleAalgorithm { get; set; } = true;
        public TimeSpan FlushingInterval { get; set; } = TimeSpan.Zero;
    }
}
