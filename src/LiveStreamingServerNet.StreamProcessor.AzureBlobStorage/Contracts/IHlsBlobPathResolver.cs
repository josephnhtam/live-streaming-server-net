
namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Contracts
{
    /// <summary>
    /// Defines methods for resolving HLS blob storage paths.
    /// </summary>
    public interface IHlsBlobPathResolver
    {
        /// <summary>
        /// Resolves a storage path for an HLS blob.
        /// </summary>
        /// <param name="context">The stream processing context.</param>
        /// <param name="fileName">Name of the file to store.</param>
        /// <returns>The resolved blob storage path.</returns>
        string ResolveBlobPath(StreamProcessingContext context, string fileName);
    }
}
