using LiveStreamingServerNet.StreamProcessor.Hls;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services.Contracts
{
    internal interface IHlsStorageEventDispatcher
    {
        Task HlsFilesStoredAsync(StreamProcessingContext context, bool initial, IReadOnlyList<StoredManifest> storedManifests, IReadOnlyList<StoredTsSegment> storedTsSegments);
        Task HlsFilesStoringCompleteAsync(StreamProcessingContext context);
    }
}
