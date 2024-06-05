using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Marshal;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Marshal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    public static class HlsTransmuxerInstaller
    {
        public static IStreamProcessingBuilder AddHlsTransmuxer(this IStreamProcessingBuilder builder, Action<HlsTransmuxerConfiguration>? configure = null)
        {
            var services = builder.Services;

            var config = new HlsTransmuxerConfiguration();
            configure?.Invoke(config);

            services.TryAddSingleton<IManifestWriter, ManifestWriter>();
            services.TryAddSingleton<IHlsTransmuxerManager, HlsTransmuxerManager>();
            services.TryAddSingleton<IHlsCleanupManager, HlsCleanupManager>();
            services.TryAddSingleton<IRtmpMediaMessageInterceptor, HlsRtmpMediaMessageScraper>();

            services.AddSingleton<IStreamProcessorFactory>(svc =>
                new HlsTransmuxerFactory(
                    svc.GetRequiredService<IHlsTransmuxerManager>(),
                    svc.GetRequiredService<IHlsCleanupManager>(),
                    svc.GetRequiredService<IManifestWriter>(),
                    config,
                    svc.GetRequiredService<ILogger<HlsTransmuxer>>(),
                    svc.GetService<IBufferPool>()
                )
            );

            return builder;
        }
    }
}
