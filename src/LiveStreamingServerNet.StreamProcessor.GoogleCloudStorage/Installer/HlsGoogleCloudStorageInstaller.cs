using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Configurations;
using LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Internal;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.Installer
{
    public static class HlsGoogleCloudStorageInstaller
    {
        public static IHlsUploaderConfigurator AddGoogleCloudStorage(
            this IHlsUploaderConfigurator configurator,
            StorageClient storageClient,
            string bucket,
            Action<HlsGoogleCloudStorageConfiguration>? configure = null)
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
