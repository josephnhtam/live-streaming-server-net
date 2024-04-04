using LiveStreamingServerNet.Transmuxer.Azure.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Azure
{
    public class DefaultHlsAzureBlobPathResolver : IHlsAzureBlobPathResolver
    {
        public string ResolveBlobPath(TransmuxingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
