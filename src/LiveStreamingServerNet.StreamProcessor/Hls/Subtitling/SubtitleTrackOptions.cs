namespace LiveStreamingServerNet.StreamProcessor.Hls.Subtitling
{
    /// <summary>
    /// Represents the configuration options for a subtitle track in an HLS stream.
    /// </summary>
    /// <param name="Name">The name or label of the subtitle track.</param>
    /// <param name="IsDefault">
    /// A value indicating whether this subtitle track should be marked as the default track.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <param name="AutoSelect">
    /// A value indicating whether the client should automatically select this subtitle track.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <param name="Language">
    /// An optional language code for the subtitle track (e.g., "en" for English).
    /// </param>
    public record SubtitleTrackOptions(string Name, bool IsDefault = true, bool AutoSelect = true, string? Language = null);
}
