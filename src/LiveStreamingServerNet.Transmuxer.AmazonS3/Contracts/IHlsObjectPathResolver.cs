
namespace LiveStreamingServerNet.Transmuxer.AmazonS3.Contracts
{
    public interface IHlsObjectPathResolver
    {
        string ResolveObjectPath(TransmuxingContext context, string fileName);
    }
}
