namespace LiveStreamingServerNet.StreamProcessor.Hls
{
    public record struct StoringResult(IReadOnlyList<StoredManifest> ManifestFiles, IReadOnlyList<StoredTsSegment> TsSegments);
}
