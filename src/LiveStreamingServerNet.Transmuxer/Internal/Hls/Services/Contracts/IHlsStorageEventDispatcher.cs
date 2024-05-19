using LiveStreamingServerNet.Transmuxer.Hls;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts
{
    internal interface IHlsStorageEventDispatcher
    {
        Task HlsFilesStoredAsync(TransmuxingContext context, bool initial, IReadOnlyList<StoredManifest> storedManifests, IReadOnlyList<StoredTsFile> storedTsFiles);
        Task HlsFilesStoringCompleteAsync(TransmuxingContext context);
    }
}
