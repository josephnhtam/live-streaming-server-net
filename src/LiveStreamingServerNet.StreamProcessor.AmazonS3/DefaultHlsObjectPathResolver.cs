using LiveStreamingServerNet.StreamProcessor.AmazonS3.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.AmazonS3
{
    public class DefaultHlsObjectPathResolver : IHlsObjectPathResolver
    {
        public string ResolveObjectPath(StreamProcessingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
