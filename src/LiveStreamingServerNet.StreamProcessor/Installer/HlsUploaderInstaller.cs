using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    public static class HlsUploaderInstaller
    {
        public static IStreamProcessingConfigurator AddHlsUploader(
            this IStreamProcessingConfigurator configurator, Action<IHlsUploaderConfigurator> configure)
        {
            var services = configurator.Services;

            services.AddSingleton<IHlsUploaderFactory, HlsUploaderFactory>()
                    .AddSingleton<IHlsUploadingManager, HlsUploadingManager>()
                    .AddSingleton<IHlsStorageEventDispatcher, HlsStorageEventDispatcher>();

            configurator.AddStreamProcessorEventHandler<HlsStreamProcessorEventListener>();

            configure(new HlsUploaderConfigurator(services));

            return configurator;
        }
    }
}
