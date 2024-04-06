using Azure.Storage.Blobs.Models;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts;

namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Configurations
{
    public class HlsAzureBlobStorageConfiguration
    {
        public BlobUploadOptions ManifestsUploadOptions { get; set; } = new BlobUploadOptions();
        public BlobUploadOptions TsFilesUploadOptions { get; set; } = new BlobUploadOptions();
        public IHlsBlobPathResolver BlobPathResolver { get; set; } = new DefaultHlsBlobPathResolver();
    }
}
