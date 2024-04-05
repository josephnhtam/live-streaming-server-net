namespace LiveStreamingServerNet.Transmuxer.Configurations
{
    public class HlsUploaderConfiguration
    {
        public int PollingIntervalMilliseconds { get; set; } = 500;
        public bool DeleteOutdatedTsFiles { get; set; } = true;

        public TimeSpan PollingInterval => TimeSpan.FromMilliseconds(PollingIntervalMilliseconds);
    }
}
