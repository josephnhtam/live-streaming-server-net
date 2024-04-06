using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Contracts;
using LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Internal;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Installer
{
    public static class HlsGoogleCloudStorageInstaller
    {
        private const string manifestCacheControl = "no-cache,no-store,max-age=0";

        public static IHlsUploaderConfigurator AddGoogleCloudStorage(
            this IHlsUploaderConfigurator configurator,
            StorageClient storageClient,
            string bucket,
            HlsUploadObjectOptions? manifestsUploadObjectOptions = null,
            HlsUploadObjectOptions? tsFilesUploadObjectOptions = null,
            IHlsObjectPathResolver? objectPathResolver = null)
        {
            var services = configurator.Services;

            services.AddSingleton<IHlsStorageAdapter>(svc =>
                new HlsGoogleCloudStorageAdapter(
                    storageClient,
                    bucket,
                    manifestsUploadObjectOptions ?? new HlsUploadObjectOptions(new UploadObjectOptions(), string.Empty),
                    tsFilesUploadObjectOptions ?? new HlsUploadObjectOptions(new UploadObjectOptions(), manifestCacheControl),
                    objectPathResolver ?? new DefaultHlsObjectPathResolver(),
                    svc.GetRequiredService<ILogger<HlsGoogleCloudStorageAdapter>>()
                )
            );

            return configurator;
        }
    }
}
