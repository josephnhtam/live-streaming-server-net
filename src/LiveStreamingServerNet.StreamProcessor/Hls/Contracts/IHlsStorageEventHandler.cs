using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    /// <summary>
    /// Defines a contract for handling HLS storage events.
    /// </summary>
    public interface IHlsStorageEventHandler
    {
        /// <summary>
        /// Gets the execution order of this handler. Lower numbers execute first.
        /// </summary>
        /// <returns>The order value, default is 0</returns>
        int GetOrder() => 0;

        /// <summary>
        /// Handles the event when HLS files have been stored in the storage system.
        /// </summary>
        /// <param name="eventContext">The context for the event being handled.</param>
        /// <param name="context">The context containing information about the stream being processed.</param>
        /// <param name="initial">Indicates whether this is the initial storage operation.</param>
        /// <param name="storedManifests">The list of manifests that were stored.</param>
        /// <param name="storedTsSegments">The list of transport stream segments that were stored.</param>
        /// <returns>A task representing the event handling operation.</returns>
        Task OnHlsFilesStoredAsync(IEventContext eventContext, StreamProcessingContext context, bool initial, IReadOnlyList<StoredManifest> storedManifests, IReadOnlyList<StoredTsSegment> storedTsSegments);

        /// <summary>
        /// Handles the event when all HLS files have been completely stored.
        /// </summary>
        /// <param name="eventContext">The context for the event being handled.</param>
        /// <param name="context">The context containing information about the stream being processed.</param>
        /// <returns>A task representing the event handling operation.</returns>
        Task OnHlsFilesStoringCompleteAsync(IEventContext eventContext, StreamProcessingContext context);
    }
}
