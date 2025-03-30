namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders
{
    public record MediaSegment(string Uri, TimeSpan Timestamp, TimeSpan Duration, string? Title = null);
}
