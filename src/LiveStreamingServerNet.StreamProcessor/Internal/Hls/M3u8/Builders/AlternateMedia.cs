namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders
{
    internal record AlternateMedia(
        string Uri,
        string Name,
        string Type,
        string GroupId,
        string? Language = null,
        bool IsDefault = false,
        bool AutoSelect = true
    );
}
