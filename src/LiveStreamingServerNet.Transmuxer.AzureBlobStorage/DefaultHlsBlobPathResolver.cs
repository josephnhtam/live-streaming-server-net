using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts;

namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage
{
    public class DefaultHlsBlobPathResolver : IHlsBlobPathResolver
    {
        public string ResolveBlobPath(TransmuxingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
