using LiveStreamingServerNet.Transmuxer.AmazonS3.Contracts;

namespace LiveStreamingServerNet.Transmuxer.AmazonS3
{
    public class DefaultHlsObjectPathResolver : IHlsObjectPathResolver
    {
        public string ResolveObjectPath(TransmuxingContext context, string fileName)
        {
            return $"{context.Identifier}/{fileName}";
        }
    }
}
