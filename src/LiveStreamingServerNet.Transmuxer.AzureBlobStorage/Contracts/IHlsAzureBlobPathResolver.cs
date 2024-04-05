
namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts
{
    public interface IHlsAzureBlobPathResolver
    {
        string ResolveBlobPath(TransmuxingContext context, string fileName);
    }
}
