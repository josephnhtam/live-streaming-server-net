using Azure.Storage.Blobs.Models;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Configurations
{
    public class HlsAzureBlobStorageConfiguration
    {
        public BlobUploadOptions ManifestsUploadOptions { get; set; } = new BlobUploadOptions();
        public BlobUploadOptions TsSegmentsUploadOptions { get; set; } = new BlobUploadOptions();
        public IHlsBlobPathResolver BlobPathResolver { get; set; } = new DefaultHlsBlobPathResolver();
    }
}
