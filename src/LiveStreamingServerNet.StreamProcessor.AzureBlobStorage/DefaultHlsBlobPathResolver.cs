using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage
{
    public class DefaultHlsBlobPathResolver : IHlsBlobPathResolver
    {
        public string ResolveBlobPath(StreamProcessingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
