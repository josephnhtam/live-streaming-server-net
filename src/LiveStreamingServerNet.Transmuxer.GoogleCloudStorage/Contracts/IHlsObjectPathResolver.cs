
namespace LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Contracts
{
    public interface IHlsObjectPathResolver
    {
        string ResolveObjectPath(TransmuxingContext context, string fileName);
    }
}
