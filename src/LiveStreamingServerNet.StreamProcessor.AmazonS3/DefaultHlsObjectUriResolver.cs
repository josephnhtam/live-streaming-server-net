
using LiveStreamingServerNet.StreamProcessor.AmazonS3.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.AmazonS3
{
    /// <summary>
    /// Default implementation for resolving HLS object URIs.
    /// </summary>
    public class DefaultHlsObjectUriResolver : IHlsObjectUriResolver
    {
        public Uri? ResolveObjectUri(string bucketName, string key)
        {
            return null;
        }
    }
}
