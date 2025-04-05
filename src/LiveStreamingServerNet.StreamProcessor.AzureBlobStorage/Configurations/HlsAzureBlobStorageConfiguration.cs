using Azure.Storage.Blobs.Models;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Configurations
{
    /// <summary>
    /// Configuration settings for Azure Blob Storage.
    /// </summary>
    public class HlsAzureBlobStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the upload options for manifest files (.m3u8).
        /// </summary>
        public BlobUploadOptions ManifestsUploadOptions { get; set; } = new BlobUploadOptions();

        /// <summary>
        /// Gets or sets the upload options for segment files.
        /// </summary>
        public BlobUploadOptions SegmentsUploadOptions { get; set; } = new BlobUploadOptions();

        /// <summary>
        /// Gets or sets the resolver for blob storage paths.
        /// </summary>
        public IHlsBlobPathResolver BlobPathResolver { get; set; } = new DefaultHlsBlobPathResolver();
    }
}
