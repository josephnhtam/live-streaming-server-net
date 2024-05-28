using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage
{
    public class DefaultHlsObjectPathResolver : IHlsObjectPathResolver
    {
        public string ResolveObjectPath(StreamProcessingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
