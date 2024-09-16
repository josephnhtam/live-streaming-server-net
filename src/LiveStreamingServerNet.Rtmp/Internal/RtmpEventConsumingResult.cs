namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal record struct RtmpEventConsumingResult(bool Succeeded, int ConsumedBytes);
}
