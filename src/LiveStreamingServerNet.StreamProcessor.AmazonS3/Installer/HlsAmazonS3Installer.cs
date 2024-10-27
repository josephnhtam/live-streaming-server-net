using Amazon.S3;
using LiveStreamingServerNet.StreamProcessor.AmazonS3.Configurations;
using LiveStreamingServerNet.StreamProcessor.AmazonS3.Internal;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AmazonS3.Installer
{
    /// <summary>
    /// Provides installation methods for Amazon S3 storage.
    /// </summary>
    public static class HlsAmazonS3Installer
    {
        /// <summary>
        /// Adds Amazon S3 storage support for HLS uploads.
        /// </summary>
        /// <param name="configurator">The HLS uploader configurator.</param>
        /// <param name="s3Client">The Amazon S3 client instance.</param>
        /// <param name="bucket">The S3 bucket name for storage.</param>
        /// <param name="configure">Optional S3 configuration.</param>
        /// <returns>The HLS uploader configurator for chaining.</returns>
        public static IHlsUploaderConfigurator AddAmazonS3(
            this IHlsUploaderConfigurator configurator, IAmazonS3 s3Client, string bucket)
            => AddAmazonS3(configurator, s3Client, bucket, null);

        /// <summary>
        /// Adds Amazon S3 storage support for HLS uploads.
        /// </summary>
        /// <param name="configurator">The HLS uploader configurator.</param>
        /// <param name="s3Client">The Amazon S3 client instance.</param>
        /// <param name="bucket">The S3 bucket name for storage.</param>
        /// <param name="configure">Optional S3 configuration.</param>
        /// <returns>The HLS uploader configurator for chaining.</returns>
        public static IHlsUploaderConfigurator AddAmazonS3(
            this IHlsUploaderConfigurator configurator,
            IAmazonS3 s3Client, string bucket,
            Action<HlsAmazonS3Configuration>? configure)
        {
            var services = configurator.Services;

            var config = new HlsAmazonS3Configuration();
            configure?.Invoke(config);

            services.AddSingleton<IHlsStorageAdapter>(svc =>
                new HlsAmazonS3Adapter(
                    s3Client,
                    bucket,
                    config,
                    svc.GetRequiredService<ILogger<HlsAmazonS3Adapter>>()
                )
            );

            return configurator;
        }
    }
}
