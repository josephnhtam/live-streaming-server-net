
namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Contracts
{
    public interface IHlsObjectPathResolver
    {
        string ResolveObjectPath(StreamProcessingContext context, string fileName);
    }
}
