using Azure.Storage.Blobs;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Configurations;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Internal;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Installer
{
    /// <summary>
    /// Provides installation methods for HLS Azure Blob Storage.
    /// </summary>
    public static class HlsAzureBlobStorageInstaller
    {
        /// <summary>
        /// Adds Azure Blob Storage support for HLS uploads.
        /// </summary>
        /// <param name="configurator">The HLS uploader configurator.</param>
        /// <param name="blobContainerClient">The Azure Blob Container client.</param>
        /// <returns>The HLS uploader configurator for chaining.</returns>
        public static IHlsUploaderConfigurator AddAzureBlobStorage(
            this IHlsUploaderConfigurator configurator, BlobContainerClient blobContainerClient)
            => AddAzureBlobStorage(configurator, blobContainerClient, null);

        /// <summary>
        /// Adds Azure Blob Storage support for HLS uploads.
        /// </summary>
        /// <param name="configurator">The HLS uploader configurator.</param>
        /// <param name="blobContainerClient">The Azure Blob Container client.</param>
        /// <param name="configure">Optional blob storage configuration.</param>
        /// <returns>The HLS uploader configurator for chaining.</returns>
        public static IHlsUploaderConfigurator AddAzureBlobStorage(
            this IHlsUploaderConfigurator configurator,
            BlobContainerClient blobContainerClient,
            Action<HlsAzureBlobStorageConfiguration>? configure)
        {
            var services = configurator.Services;

            var config = new HlsAzureBlobStorageConfiguration();
            configure?.Invoke(config);

            services.AddSingleton<IHlsStorageAdapter>(svc =>
                new HlsAzureBlobStorageAdapter(
                    blobContainerClient,
                    config,
                    svc.GetRequiredService<ILogger<HlsAzureBlobStorageAdapter>>()
                )
            );

            return configurator;
        }
    }
}
