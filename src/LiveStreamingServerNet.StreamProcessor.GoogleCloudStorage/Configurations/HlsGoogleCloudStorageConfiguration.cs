using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Configurations
{
    /// <summary>
    /// Configuration settings for Google Cloud Storage.
    /// </summary>
    public class HlsGoogleCloudStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the upload options for manifest files (.m3u8).
        /// Default: new UploadObjectOptions()
        /// </summary>
        public UploadObjectOptions ManifestsUploadObjectOptions { get; set; } = new UploadObjectOptions();

        /// <summary>
        /// Gets or sets the upload options for TS segment files (.ts).
        /// Default: new UploadObjectOptions()
        /// </summary>
        public UploadObjectOptions TsSegmentsUploadObjectOptions { get; set; } = new UploadObjectOptions();

        /// <summary>
        /// Gets or sets the Cache-Control header value for manifest files.
        /// Default: "no-cache,no-store,max-age=0"
        /// </summary>
        public string ManifestsCacheControl { get; set; } = "no-cache,no-store,max-age=0";

        /// <summary>
        /// Gets or sets the Cache-Control header value for TS segment files.
        /// Default: empty string
        /// </summary>
        public string TsSegmentsCacheControl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resolver for object storage paths.
        /// </summary>
        public IHlsObjectPathResolver ObjectPathResolver { get; set; } = new DefaultHlsObjectPathResolver();

        /// <summary>
        /// Gets or sets the resolver for object URIs.
        /// </summary>
        public IHlsObjectUriResolver ObjectUriResolver { get; set; } = new DefaultHlsObjectUriResolver();
    }
}
