namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    public interface IHlsStorageAdapter
    {
        Task<StoringResult> StoreAsync(StreamProcessingContext context, IReadOnlyList<Manifest> manifests, IReadOnlyList<ManifestTsSegment> tsSegments, CancellationToken cancellationToken);
        Task DeleteAsync(StreamProcessingContext context, IReadOnlyList<ManifestTsSegment> tsSegments, CancellationToken cancellationToken);
    }
}
