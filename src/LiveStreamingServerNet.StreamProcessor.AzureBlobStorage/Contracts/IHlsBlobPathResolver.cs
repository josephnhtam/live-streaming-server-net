
namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Contracts
{
    public interface IHlsBlobPathResolver
    {
        string ResolveBlobPath(StreamProcessingContext context, string fileName);
    }
}
