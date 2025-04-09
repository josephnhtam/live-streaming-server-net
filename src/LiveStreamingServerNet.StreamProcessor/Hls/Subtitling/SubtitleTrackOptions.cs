namespace LiveStreamingServerNet.StreamProcessor.Hls.Subtitling
{
    public record SubtitleTrackOptions(string Name, bool IsDefault = true, bool AutoSelect = true, string? Language = null);
}
