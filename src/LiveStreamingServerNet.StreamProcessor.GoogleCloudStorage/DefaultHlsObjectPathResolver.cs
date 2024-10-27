using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage
{
    /// <summary>
    /// Default implementation for resolving GCS object paths in format: {identifier}/{filename}.
    /// </summary>
    public class DefaultHlsObjectPathResolver : IHlsObjectPathResolver
    {
        public string ResolveObjectPath(StreamProcessingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
