namespace LiveStreamingServerNet.StreamProcessor.Hls
{
    public record struct ManifestTsSegment(string ManifestName, string FileName, float Duration);
}
