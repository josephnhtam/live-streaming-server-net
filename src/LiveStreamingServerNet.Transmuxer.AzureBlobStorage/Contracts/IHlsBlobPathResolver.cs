
namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts
{
    public interface IHlsBlobPathResolver
    {
        string ResolveBlobPath(TransmuxingContext context, string fileName);
    }
}
