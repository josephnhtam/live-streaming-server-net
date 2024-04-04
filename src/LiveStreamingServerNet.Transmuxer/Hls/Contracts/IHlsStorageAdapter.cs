namespace LiveStreamingServerNet.Transmuxer.Hls.Contracts
{
    public interface IHlsStorageAdapter
    {
        Task<StoringResult> StoreAsync(TransmuxingContext context, IReadOnlyList<Manifest> manifests, IReadOnlyList<TsFile> tsFiles, CancellationToken cancellationToken);
    }
}
