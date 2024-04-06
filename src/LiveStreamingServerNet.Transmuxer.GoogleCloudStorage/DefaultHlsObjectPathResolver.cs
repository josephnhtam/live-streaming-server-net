using LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Contracts;

namespace LiveStreamingServerNet.Transmuxer.GoogleCloudStorage
{
    public class DefaultHlsObjectPathResolver : IHlsObjectPathResolver
    {
        public string ResolveObjectPath(TransmuxingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
