namespace LiveStreamingServerNet.Transmuxer.Hls
{
    public record struct StoringResult(IReadOnlyList<StoredManifest> ManifestFiles, IReadOnlyList<StoredTsFile> TsFiles);
}
