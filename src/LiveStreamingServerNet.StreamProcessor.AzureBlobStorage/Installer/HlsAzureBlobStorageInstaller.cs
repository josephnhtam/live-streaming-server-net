using Azure.Storage.Blobs;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Configurations;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Internal;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Installer
{
    public static class HlsAzureBlobStorageInstaller
    {
        public static IHlsUploaderConfigurator AddAzureBlobStorage(
            this IHlsUploaderConfigurator configurator,
            BlobContainerClient blobContainerClient,
            Action<HlsAzureBlobStorageConfiguration>? configure = null)
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
