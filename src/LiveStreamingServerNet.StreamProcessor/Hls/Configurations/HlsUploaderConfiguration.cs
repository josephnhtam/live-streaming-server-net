namespace LiveStreamingServerNet.StreamProcessor.Configurations
{
    public class HlsUploaderConfiguration
    {
        public int PollingIntervalMilliseconds { get; set; } = 500;
        public bool DeleteOutdatedTsSegments { get; set; } = true;

        public TimeSpan PollingInterval => TimeSpan.FromMilliseconds(PollingIntervalMilliseconds);
    }
}
