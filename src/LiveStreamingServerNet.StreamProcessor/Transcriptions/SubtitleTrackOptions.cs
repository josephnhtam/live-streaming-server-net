namespace LiveStreamingServerNet.StreamProcessor.Transcriptions
{
    public record struct SubtitleTrackOptions(string? Name, string? Language, bool IsDefault = true, bool AutoSelect = true);
}
