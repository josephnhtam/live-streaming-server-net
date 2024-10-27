using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    /// <summary>
    /// Provides extension methods for installing and configuring HLS uploading services.
    /// </summary>
    public static class HlsUploaderInstaller
    {
        /// <summary>
        /// Adds HLS uploading services.
        /// </summary>
        /// <param name="configurator">The stream processing configurator to add services to.</param>
        /// <param name="configure">Action to configure the HLS uploader settings.</param>
        /// <returns>The stream processing configurator for method chaining.</returns>
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
