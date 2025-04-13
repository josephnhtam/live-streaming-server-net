namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    /// <summary>
    /// Defines a contract for storing and managing HLS content in a storage system.
    /// </summary>
    public interface IHlsStorageAdapter
    {
        /// <summary>
        /// Stores HLS manifests and media segments in the storage system.
        /// </summary>
        /// <param name="context">The context containing information about the stream being processed.</param>
        /// <param name="manifests">The list of HLS manifests to store.</param>
        /// <param name="segments">The list of media segments to store.</param>
        /// <param name="cancellationToken">Token to cancel the storage operation.</param>
        /// <returns>A task containing the result of the storing operation.</returns>
        Task<StoringResult> StoreAsync(StreamProcessingContext context, IReadOnlyList<Manifest> manifests, IReadOnlyList<Segment> segments, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes specified media segments from the storage system.
        /// </summary>
        /// <param name="context">The context containing information about the stream being processed.</param>
        /// <param name="segments">The list of media segments to delete.</param>
        /// <param name="cancellationToken">Token to cancel the deletion operation.</param>
        /// <returns>A task representing the deletion operation.</returns>
        Task DeleteAsync(StreamProcessingContext context, IReadOnlyList<Segment> segments, CancellationToken cancellationToken);
    }
}
