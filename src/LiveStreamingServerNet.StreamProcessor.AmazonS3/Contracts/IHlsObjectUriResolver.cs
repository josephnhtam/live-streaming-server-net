
namespace LiveStreamingServerNet.StreamProcessor.AmazonS3.Contracts
{
    /// <summary>
    /// Defines methods for resolving HLS object URIs.
    /// </summary>
    public interface IHlsObjectUriResolver
    {
        /// <summary>
        /// Resolves a URI for an HLS object in storage.
        /// </summary>
        /// <param name="bucketName">The storage bucket name.</param>
        /// <param name="key">The object key in storage.</param>
        /// <returns>The resolved URI, or null if resolution fails.</returns>
        Uri? ResolveObjectUri(string bucketName, string key);
    }
}
