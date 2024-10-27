
namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Contracts
{
    /// <summary>
    /// Defines methods for resolving HLS object storage paths.
    /// </summary>
    public interface IHlsObjectPathResolver
    {
        /// <summary>
        /// Resolves a storage path for an HLS object.
        /// </summary>
        /// <param name="context">The stream processing context.</param>
        /// <param name="fileName">Name of the file to store.</param>
        /// <returns>The resolved storage path.</returns>
        string ResolveObjectPath(StreamProcessingContext context, string fileName);
    }
}
