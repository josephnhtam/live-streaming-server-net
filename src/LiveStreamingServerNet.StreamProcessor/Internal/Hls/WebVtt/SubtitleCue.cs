namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.WebVtt
{
    public record struct SubtitleCue(string Text, TimeSpan Timestamp, TimeSpan Duration);
}
