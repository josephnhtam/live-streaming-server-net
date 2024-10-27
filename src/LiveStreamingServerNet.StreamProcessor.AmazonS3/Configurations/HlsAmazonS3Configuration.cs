using LiveStreamingServerNet.StreamProcessor.AmazonS3.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.AmazonS3.Configurations
{
    /// <summary>
    /// Configuration settings for Amazon S3 storage.
    /// </summary>
    public class HlsAmazonS3Configuration
    {
        /// <summary>
        /// Gets or sets the resolver for S3 object paths.
        /// </summary>
        public IHlsObjectPathResolver ObjectPathResolver { get; set; } = new DefaultHlsObjectPathResolver();

        /// <summary>
        /// Gets or sets the resolver for S3 object URIs.
        /// </summary>
        public IHlsObjectUriResolver ObjectUriResolver { get; set; } = new DefaultHlsObjectUriResolver();
    }
}
