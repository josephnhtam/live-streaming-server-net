using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Configurations;
using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Internal;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Installer
{
    /// <summary>
    /// Provides installation methods for HLS Google Cloud Storage.
    /// </summary>
    public static class HlsGoogleCloudStorageInstaller
    {
        /// <summary>
        /// Adds Google Cloud Storage support for HLS uploads.
        /// </summary>
        /// <param name="configurator">The HLS uploader configurator.</param>
        /// <param name="storageClient">The Google Cloud Storage client.</param>
        /// <param name="bucket">The storage bucket name.</param>
        /// <returns>The HLS uploader configurator for chaining.</returns>
        public static IHlsUploaderConfigurator AddGoogleCloudStorage(
            this IHlsUploaderConfigurator configurator, StorageClient storageClient, string bucket)
            => AddGoogleCloudStorage(configurator, storageClient, bucket, null);

        /// <summary>
        /// Adds Google Cloud Storage support for HLS uploads.
        /// </summary>
        /// <param name="configurator">The HLS uploader configurator.</param>
        /// <param name="storageClient">The Google Cloud Storage client.</param>
        /// <param name="bucket">The storage bucket name.</param>
        /// <param name="configure">Optional cloud storage configuration.</param>
        /// <returns>The HLS uploader configurator for chaining.</returns>
        public static IHlsUploaderConfigurator AddGoogleCloudStorage(
            this IHlsUploaderConfigurator configurator,
            StorageClient storageClient,
            string bucket,
            Action<HlsGoogleCloudStorageConfiguration>? configure)
        {
            var services = configurator.Services;

            var config = new HlsGoogleCloudStorageConfiguration();
            configure?.Invoke(config);

            services.AddSingleton<IHlsStorageAdapter>(svc =>
                new HlsGoogleCloudStorageAdapter(
                    storageClient,
                    bucket,
                    config,
                    svc.GetRequiredService<ILogger<HlsGoogleCloudStorageAdapter>>()
                )
            );

            return configurator;
        }
    }
}
