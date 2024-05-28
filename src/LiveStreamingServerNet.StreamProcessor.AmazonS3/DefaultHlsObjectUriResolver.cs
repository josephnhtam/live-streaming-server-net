
using LiveStreamingServerNet.StreamProcessor.AmazonS3.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.AmazonS3
{
    public class DefaultHlsObjectUriResolver : IHlsObjectUriResolver
    {
        public Uri? ResolveObjectUri(string bucketName, string key)
        {
            return null;
        }
    }
}
