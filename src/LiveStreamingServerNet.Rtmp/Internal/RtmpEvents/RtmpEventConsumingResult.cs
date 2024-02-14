namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEvents
{
    internal record struct RtmpEventConsumingResult(bool Succeeded, int ConsumedBytes);
}
