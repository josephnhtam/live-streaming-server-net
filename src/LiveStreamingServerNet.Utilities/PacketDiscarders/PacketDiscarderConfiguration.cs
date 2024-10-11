namespace LiveStreamingServerNet.Utilities.PacketDiscarders
{
    public class PacketDiscarderConfiguration
    {
        public int TargetOutstandingPacketsCount { get; set; } = 64;
        public long TargetOutstandingPacketsSize { get; set; } = 1024 * 1024;
        public int MaxOutstandingPacketsCount { get; set; } = 512;
        public long MaxOutstandingPacketsSize { get; set; } = 8 * 1024 * 1024;
    }
}
