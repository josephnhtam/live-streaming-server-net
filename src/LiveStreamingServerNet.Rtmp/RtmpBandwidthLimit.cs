namespace LiveStreamingServerNet.Rtmp
{
    public record struct RtmpBandwidthLimit(uint Bandwidth, RtmpBandwidthLimitType LimitType);

    public enum RtmpBandwidthLimitType : byte
    {
        Hard = 0,
        Soft = 1,
        Dynamic = 2
    }
}
