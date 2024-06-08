using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    public interface IHlsStorageEventHandler
    {
        int GetOrder() => 0;
        Task OnHlsFilesStoredAsync(IEventContext eventContext, StreamProcessingContext context, bool initial, IReadOnlyList<StoredManifest> storedManifests, IReadOnlyList<StoredTsSegment> storedTsSegments);
        Task OnHlsFilesStoringCompleteAsync(IEventContext eventContext, StreamProcessingContext context);
    }
}
