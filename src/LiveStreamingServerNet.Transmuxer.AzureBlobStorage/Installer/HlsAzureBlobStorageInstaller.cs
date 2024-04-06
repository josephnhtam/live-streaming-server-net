using Azure.Storage.Blobs;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer.Contracts;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Internal;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer
{
    public static class HlsAzureBlobStorageInstaller
    {
        public static IHlsUploaderConfigurator AddAzureBlobStorage(
            this IHlsUploaderConfigurator configurator,
            BlobContainerClient blobContainerClient,
            Action<IHlsAzureBlobStorageConfigurator>? configure = null)
        {
            var services = configurator.Services;

            services.AddKeyedSingleton("hls-blob-container-client", blobContainerClient)
                    .AddSingleton<IHlsStorageAdapter, HlsAzureBlobStorageAdapter>();

            if (configure != null)
                configure.Invoke(new HlsAzureBlobStorageConfigurator(services));

            services.TryAddSingleton<IHlsBlobPathResolver, DefaultHlsBlobPathResolver>();

            return configurator;
        }
    }
}
