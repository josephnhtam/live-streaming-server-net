using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
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
    /// <summary>
    /// Provides extension methods for installing and configuring HLS transmuxing services.
    /// </summary>
    public static class HlsTransmuxerInstaller
    {
        /// <summary>
        /// Adds HLS transmuxing to the stream processing.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddHlsTransmuxer(this IStreamProcessingBuilder builder)
            => AddHlsTransmuxer(builder, null);

        /// <summary>
        /// Adds HLS transmuxing to the stream processing.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <param name="configure">Optional action to configure the HLS transmuxer.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddHlsTransmuxer(this IStreamProcessingBuilder builder, Action<HlsTransmuxerConfiguration>? configure)
        {
            var services = builder.Services;

            var config = new HlsTransmuxerConfiguration();
            configure?.Invoke(config);

            services.TryAddSingleton<IHlsPathRegistry, HlsPathRegistry>();
            services.TryAddSingleton<IHlsPathMapper>(svc => svc.GetRequiredService<IHlsPathRegistry>());

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
                    svc.GetRequiredService<IHlsPathRegistry>(),
                    config,
                    svc.GetRequiredService<ILogger<HlsTransmuxer>>(),
                    svc.GetService<IBufferPool>()
                )
            );

            return builder;
        }
    }
}
