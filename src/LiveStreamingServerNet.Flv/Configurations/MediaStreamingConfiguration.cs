﻿namespace LiveStreamingServerNet.Flv.Configurations
{
    public class MediaStreamingConfiguration
    {
        public int TargetOutstandingMediaPacketsCount { get; set; } = 64;
        public long TargetOutstandingMediaPacketsSize { get; set; } = 1024 * 1024;
        public int MaxOutstandingMediaPacketsCount { get; set; } = 512;
        public long MaxOutstandingMediaPacketsSize { get; set; } = 8 * 1024 * 1024;
        public TimeSpan ReadinessTimeout { get; set; } = TimeSpan.FromSeconds(15);
    }
}
