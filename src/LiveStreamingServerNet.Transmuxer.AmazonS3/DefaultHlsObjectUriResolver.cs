
using LiveStreamingServerNet.Transmuxer.AmazonS3.Contracts;

namespace LiveStreamingServerNet.Transmuxer.AmazonS3
{
    public class DefaultHlsObjectUriResolver : IHlsObjectUriResolver
    {
        public Uri? ResolveObjectUri(string bucketName, string key)
        {
            return null;
        }
    }
}
