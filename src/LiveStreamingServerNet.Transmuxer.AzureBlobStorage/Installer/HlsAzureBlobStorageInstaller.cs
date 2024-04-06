using Azure.Storage.Blobs;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts;
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
            IHlsBlobPathResolver? blobPathResolver = null)
        {
            var services = configurator.Services;

            services.AddSingleton<IHlsStorageAdapter>(svc =>
                new HlsAzureBlobStorageAdapter(
                    blobContainerClient,
                    blobPathResolver ?? new DefaultHlsBlobPathResolver(),
                    svc.GetRequiredService<ILogger<HlsAzureBlobStorageAdapter>>()
                )
            );

            return configurator;
        }
    }
}
