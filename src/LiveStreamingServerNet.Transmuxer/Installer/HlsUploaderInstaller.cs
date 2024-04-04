using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer
{
    public static class HlsUploaderInstaller
    {
        public static ITransmuxerConfigurator AddHlsUploader(
            this ITransmuxerConfigurator configurator, Action<IHlsUploaderConfigurator> configure)
        {
            var services = configurator.Services;

            services.AddSingleton<IHlsUploaderFactory, HlsUploaderFactory>()
                    .AddSingleton<IHlsUploadingManager, HlsUploadingManager>()
                    .AddSingleton<IHlsStorageEventDispatcher, HlsStorageEventDispatcher>();

            configurator.AddTransmuxerEventHandler<HlsTransmuxerEventListener>();

            configure(new HlsUploaderConfigurator(services));

            return configurator;
        }
    }
}
