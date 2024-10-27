namespace LiveStreamingServerNet.Rtmp
{
    /// <summary>
    /// Represents bandwidth limit settings for RTMP connections.
    /// </summary>
    /// <param name="Bandwidth">Maximum bandwidth in bytes per second.</param>
    /// <param name="LimitType">Type of bandwidth limitation to apply.</param>
    public record struct RtmpBandwidthLimit(uint Bandwidth, RtmpBandwidthLimitType LimitType);

    /// <summary>
    /// Types of bandwidth limitations that can be applied to RTMP connections.
    /// </summary>
    public enum RtmpBandwidthLimitType : byte
    {
        Hard = 0,
        Soft = 1,
        Dynamic = 2
    }
}
