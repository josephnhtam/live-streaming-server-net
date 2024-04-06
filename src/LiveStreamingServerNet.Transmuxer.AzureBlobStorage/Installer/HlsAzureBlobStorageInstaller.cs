using Azure.Storage.Blobs;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Configurations;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Internal;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer
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
