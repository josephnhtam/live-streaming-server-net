using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
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

            configurator.AddStreamProcessorEventHandler<HlsTransmuxerEventListener>();

            configure(new HlsUploaderConfigurator(services));

            return configurator;
        }
    }
}
