namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Contracts
{
    /// <summary>
    /// Defines methods for resolving URIs for HLS objects in Google Cloud Storage.
    /// </summary>
    public interface IHlsObjectUriResolver
    {
        /// <summary>
        /// Resolves a URI for a Google Cloud Storage object.
        /// </summary>
        /// <param name="object">The Google Cloud Storage object.</param>
        /// <returns>The resolved URI, or null if resolution fails.</returns>
        Uri? ResolveObjectUri(Google.Apis.Storage.v1.Data.Object @object);
    }
}
