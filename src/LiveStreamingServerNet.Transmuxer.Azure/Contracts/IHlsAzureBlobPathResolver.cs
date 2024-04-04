
namespace LiveStreamingServerNet.Transmuxer.Azure.Contracts
{
    public interface IHlsAzureBlobPathResolver
    {
        string ResolveBlobPath(TransmuxingContext context, string fileName);
    }
}
