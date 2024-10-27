using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage
{
    /// <summary>
    /// Default implementation for resolving HLS blob paths in format: {identifier}/{filename}
    /// </summary>
    public class DefaultHlsBlobPathResolver : IHlsBlobPathResolver
    {
        public string ResolveBlobPath(StreamProcessingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
