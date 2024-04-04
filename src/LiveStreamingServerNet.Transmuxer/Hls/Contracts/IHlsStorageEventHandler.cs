using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Hls.Contracts
{
    public interface IHlsStorageEventHandler
    {
        int GetOrder() => 0;
        Task OnHlsFilesStoredAsync(IEventContext eventContext, TransmuxingContext context, bool initial, IReadOnlyList<StoredManifest> storedManifests, IReadOnlyList<StoredTsFile> storedTsFiles);
        Task OnHlsFilesStoringCompleteAsync(IEventContext eventContext, TransmuxingContext context);
    }
}
