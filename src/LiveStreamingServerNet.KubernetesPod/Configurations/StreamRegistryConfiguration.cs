namespace LiveStreamingServerNet.KubernetesPod.Configurations
{
    public class StreamRegistryConfiguration
    {
        public int KeepaliveIntervalSeconds { get; set; } = 30;
        public int KeepaliveRetryDelaySeconds { get; set; } = 5;
        public int KeepaliveTimeoutSeconds { get; set; } = 60;
        public int KeepaliveToleranceSeconds { get; set; } = 5;

        public TimeSpan KeepaliveInterval => TimeSpan.FromSeconds(KeepaliveIntervalSeconds);
        public TimeSpan KeepaliveRetryDelay => TimeSpan.FromSeconds(KeepaliveRetryDelaySeconds);
        public TimeSpan KeepaliveTimeout => TimeSpan.FromSeconds(KeepaliveTimeoutSeconds);
        public TimeSpan KeepaliveTolerance => TimeSpan.FromSeconds(KeepaliveToleranceSeconds);
    }
}
