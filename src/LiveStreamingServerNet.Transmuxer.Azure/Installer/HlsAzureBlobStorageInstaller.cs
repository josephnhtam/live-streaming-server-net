using Azure.Storage.Blobs;
using LiveStreamingServerNet.Transmuxer.Azure.Contracts;
using LiveStreamingServerNet.Transmuxer.Azure.Installer.Contracts;
using LiveStreamingServerNet.Transmuxer.Azure.Internal;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Transmuxer.Azure.Installer
{
    public static class HlsAzureBlobStorageInstaller
    {
        public static IHlsUploaderConfigurator AddAzureBlobStorage(
            this IHlsUploaderConfigurator configurator,
            BlobContainerClient blobContainerClient,
            Action<IHlsAzureStorageConfigurator>? configure = null)
        {
            var services = configurator.Services;

            services.AddKeyedSingleton("hls-blob-container-client", blobContainerClient)
                    .AddSingleton<IHlsStorageAdapter, HlsAzureBlobStorageAdapter>();

            if (configure != null)
                configure.Invoke(new HlsAzureBlobStorageConfigurator(services));

            services.TryAddSingleton<IHlsAzureBlobPathResolver, DefaultHlsAzureBlobPathResolver>();

            return configurator;
        }
    }
}
