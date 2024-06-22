using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.M3u8;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.M3u8.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
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
            services.AddSingleton<IRtmpMediaMessageInterceptor, HlsRtmpMediaMessageScraper>();

            services.AddSingleton<IStreamProcessorFactory>(svc =>
                new HlsTransmuxerFactory(
                    svc,
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
