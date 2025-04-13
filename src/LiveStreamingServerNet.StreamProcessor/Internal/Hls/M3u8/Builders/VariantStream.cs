namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders
{
    internal record VariantStream(
        string Uri,
        string Name = "Default",
        int? Bandwidth = null,
        string? Resolution = null,
        string? Codecs = null,
        string? Language = null,
        string? GroupId = null,
        IDictionary<string, string>? ExtraAttributes = null
    );
}
