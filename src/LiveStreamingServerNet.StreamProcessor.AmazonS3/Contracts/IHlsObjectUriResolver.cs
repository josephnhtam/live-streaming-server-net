
namespace LiveStreamingServerNet.StreamProcessor.AmazonS3.Contracts
{
    public interface IHlsObjectUriResolver
    {
        Uri? ResolveObjectUri(string bucketName, string key);
    }
}
