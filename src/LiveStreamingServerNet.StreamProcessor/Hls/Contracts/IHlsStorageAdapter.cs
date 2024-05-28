namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    public interface IHlsStorageAdapter
    {
        Task<StoringResult> StoreAsync(StreamProcessingContext context, IReadOnlyList<Manifest> manifests, IReadOnlyList<TsFile> tsFiles, CancellationToken cancellationToken);
        Task DeleteAsync(StreamProcessingContext context, IReadOnlyList<TsFile> tsFiles, CancellationToken cancellationToken);
    }
}
