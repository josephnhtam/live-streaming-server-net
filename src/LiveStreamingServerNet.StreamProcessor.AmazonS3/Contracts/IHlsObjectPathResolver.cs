
namespace LiveStreamingServerNet.StreamProcessor.AmazonS3.Contracts
{
    public interface IHlsObjectPathResolver
    {
        string ResolveObjectPath(StreamProcessingContext context, string fileName);
    }
}
