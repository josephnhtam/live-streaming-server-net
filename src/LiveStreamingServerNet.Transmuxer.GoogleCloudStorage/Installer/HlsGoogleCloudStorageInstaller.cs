using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Configurations;
using LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Internal;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Installer
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
